using System.ComponentModel.DataAnnotations;

namespace LogAggregator.Backend.Database.Models;

public class LoginModel
{
	[Required(ErrorMessage = "Username is required")]
	public string Username { get; set; } = "";

	[Required(ErrorMessage = "Password is required")]
	public string Password { get; set; } = "";
}
