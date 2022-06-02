using LogAggregator.Backend.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LogAggregator.Backend.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : Controller
{
	private UserManager<IdentityUser> UserManager { get; }
	private IConfiguration Configuration { get; }

	public UserController
	(
		UserManager<IdentityUser> userManager,
		IConfiguration configuration
	)
	{
		UserManager = userManager;
		Configuration = configuration;
	}

	[Authorize(Roles = UserRoles.Admin)]
	[HttpPost]
	public async Task<IActionResult> Register([FromBody] RegisterModel model)
	{
		IdentityUser? userExists = await UserManager.FindByNameAsync(model.Username);

		if (userExists is not null)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new Response
			{
				Status = "Error",
				Message = "User already exists!"
			});
		}

		IdentityUser user = new()
		{
			Email = model.Email,
			SecurityStamp = Guid.NewGuid().ToString(),
			UserName = model.Username
		};

		IdentityResult result = await UserManager.CreateAsync(user, model.Password);

		if (!result.Succeeded)
		{
			return StatusCode(StatusCodes.Status500InternalServerError, new Response
			{
				Status = "Error",
				Message = "User creation failed! Please check user details and try again.",
			});
		}

		return Ok(new Response { Status = "Success", Message = "User created successfully!" });
	}

	[HttpPost]
	public async Task<IActionResult> Login([FromBody] LoginModel model)
	{
		IdentityUser? user = await UserManager.FindByNameAsync(model.Username);

		if (user is null || !await UserManager.CheckPasswordAsync(user, model.Password))
		{
			return Unauthorized();
		}

		IList<string> userRoles = await UserManager.GetRolesAsync(user);

		List<Claim> authClaims = new()
		{
			new Claim(ClaimTypes.Name, user.UserName),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
		};

		foreach (string userRole in userRoles)
		{
			authClaims.Add(new Claim(ClaimTypes.Role, userRole));
		}

		SymmetricSecurityKey authSigningKey = new(Encoding.UTF8.GetBytes(Configuration["JWT:SecretKey"]));

		JwtSecurityToken token = new
		(
			issuer: Configuration["JWT:ValidIssuer"],
			audience: Configuration["JWT:ValidAudience"],
			expires: DateTime.Now.AddHours(3),
			claims: authClaims,
			signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
		);

		return Ok(new LoginResponseModel
		{
			Token = new JwtSecurityTokenHandler().WriteToken(token),
			Expiration = token.ValidTo
		});
	}
}
