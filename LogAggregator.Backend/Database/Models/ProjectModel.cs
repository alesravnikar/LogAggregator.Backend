namespace LogAggregator.Backend.Database.Models;

public class ProjectModel
{
	public Guid Id { get; set; }
	public string Name { get; set; } = "";
	public List<LogModel> Logs { get; set; } = new();
}
