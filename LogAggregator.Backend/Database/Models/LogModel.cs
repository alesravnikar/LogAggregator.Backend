namespace LogAggregator.Backend.Database.Models;

public class LogModel
{
	public int Id { get; set; }
	public LogSeverityLevel Level { get; set; }
	public DateTime TimeStamp { get; set; }
	public string Content { get; set; } = "";
	// todo: this should be linked to actual users?
	public string User { get; set; } = "";
}
