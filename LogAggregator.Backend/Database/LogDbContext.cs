using LogAggregator.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LogAggregator.Backend.Database;

public class LogDbContext : DbContext
{
	public DbSet<ProjectModel> Projects { get; set; } = default!;
	public DbSet<LogModel> Logs { get; set; } = default!;

	public LogDbContext(DbContextOptions<LogDbContext> options) : base(options)
	{

	}
}
