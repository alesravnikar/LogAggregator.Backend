using System.Text.Json.Serialization;

namespace LogAggregator.Backend.Database.Models;

public class LoginResponseModel
{
	[JsonPropertyName("token")]
	public string Token { get; set; } = "";
	[JsonPropertyName("expiration")]
	public DateTime Expiration { get; set; }
}
