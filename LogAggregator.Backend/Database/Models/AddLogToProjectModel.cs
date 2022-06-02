namespace LogAggregator.Backend.Database.Models;

public class AddLogToProjectModel
{
	public Guid ProjectId { get; set; }
	public LogSeverityLevel LogSeverityLevel { get; set; }
	public string Content { get; set; } = "";
	public string User { get; set; } = "";
}
