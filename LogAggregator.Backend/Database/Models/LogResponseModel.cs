namespace LogAggregator.Backend.Database.Models;

public class LogResponseModel
{
	public LogSeverityLevel Level { get; set; }
	public DateTime TimeStamp { get; set; }
	public string Content { get; set; } = "";
}
