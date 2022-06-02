namespace LogAggregator.Backend.Database.Models;

public class ProjectStatisticsResponse
{
	public int LogsInLastHour { get; set; }
	public int LogsInLast24Hours { get; set; }
	public Dictionary<LogSeverityLevel, int> LogCountBySeverity { get; set; } = new();
}