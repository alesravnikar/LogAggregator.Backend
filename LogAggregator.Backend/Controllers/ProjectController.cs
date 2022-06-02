using LogAggregator.Backend.Database;
using LogAggregator.Backend.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text;

namespace LogAggregator.Backend.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class ProjectController : Controller
{
	public LogDbContext LogDb { get; }

	public ProjectController(LogDbContext logDb)
	{
		LogDb = logDb;
	}

	[HttpPost]
	public async Task<IActionResult> AddProject([FromBody] ProjectCreateModel project)
	{
		await LogDb.Projects.AddAsync(new()
		{
			Id = Guid.NewGuid(),
			Name = project.Name,
		});

		await LogDb.SaveChangesAsync();

		return  Ok(new Response { Status = "Success", Message = "Project created successfully!" });
	}

	[HttpGet]
	public IActionResult GetProjects()
	{
		return Ok(LogDb.Projects.Select(p => new ProjectResponseModel
		{
			Id = p.Id,
			Name = p.Name,
		}));
	}

	// todo: I'm guessing there should be a "project" user group, and only projects should be able to add logs
	[HttpPost]
	public async Task<IActionResult> AddLogToProject([FromBody] AddLogToProjectModel log)
	{
		ProjectModel? project =  LogDb.Projects.FirstOrDefault(p => p.Id == log.ProjectId);

		if (project is null) return BadRequest("Invalid project");

		EntityEntry<LogModel>? newLog = await LogDb.Logs.AddAsync(new()
		{
			Level = log.LogSeverityLevel,
			Content = log.Content,
			// todo: should the project declare when the log was created?
			TimeStamp = DateTime.Now,
			User = log.User,
		});

		project.Logs.Add(newLog.Entity);

		await LogDb.SaveChangesAsync();

		return Ok(new Response { Status = "Success", Message = "Log added successfully!" });
	}

	[HttpGet]
	public IActionResult GetProjectLogs(Guid projectId)
	{
		ProjectModel? project = LogDb.Projects
			.Include(p => p.Logs)
			.FirstOrDefault(p => p.Id == projectId);

		if (project is null) return BadRequest("Invalid project");

		return Ok(project.Logs.Select(l => new LogResponseModel
		{
			Level = l.Level,
			Content = l.Content,
			TimeStamp = l.TimeStamp,
		}));
	}

	[HttpGet]
	public IActionResult GetProjectStatistics(Guid projectId)
	{
		ProjectModel? project = LogDb.Projects
			.Include(p => p.Logs)
			.FirstOrDefault(p => p.Id == projectId);

		if (project is null) return BadRequest("Invalid project");

		int logsInLast24Hours = project.Logs
			.Count(l => l.TimeStamp > (DateTime.Now - TimeSpan.FromDays(1)));

		int logsInLastHour = project.Logs
			.Count(l => l.TimeStamp > (DateTime.Now - TimeSpan.FromHours(1)));

		Dictionary<LogSeverityLevel, int> logCountBySeverity = project.Logs
			.GroupBy(l => l.Level)
			.ToDictionary(g => g.Key, g => g.Count());

		return Ok(new ProjectStatisticsResponse
		{
			LogsInLastHour = logsInLastHour,
			LogsInLast24Hours = logsInLast24Hours,
			LogCountBySeverity = logCountBySeverity,
		});
	}

	[HttpGet]
	// hack: this is just a csv export which should be good enough in this case
	public IActionResult ExportLogsToExcel(Guid projectId)
	{
		ProjectModel? project = LogDb.Projects
			.Include(p => p.Logs)
			.FirstOrDefault(p => p.Id == projectId);

		if (project is null) return BadRequest("Invalid project");

		// todo: this will break for any non-trivial scenario
		IEnumerable<string> lines = project.Logs.Select(l => $"{l.Level},{l.TimeStamp},{l.Content}");
		string csv = string.Join(Environment.NewLine, lines);

		return File(Encoding.UTF8.GetBytes(csv), "text/csv", "logs.csv");
	}
}
