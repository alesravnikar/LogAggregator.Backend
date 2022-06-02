using LogAggregator.Backend.Database;
using Microsoft.AspNetCore.Identity;

namespace LogAggregator.Backend;

public class DataSeeder
{
	public UserDbContext UserDb { get; }
	public RoleManager<IdentityRole> RoleManager { get; }
	public UserManager<IdentityUser> UserManager { get; }

	public DataSeeder
	(
		UserDbContext userDb,
		RoleManager<IdentityRole> roleManager,
		UserManager<IdentityUser> userManager
	)
	{ 
		UserDb = userDb;
		RoleManager = roleManager;
		UserManager = userManager;
	}

	public async Task Seed()
	{
		if (UserManager.Users.Any()) return;

		string[] roleNames = { UserRoles.Admin, UserRoles.User };

		foreach (string roleName in roleNames)
		{
			if (!await RoleManager.RoleExistsAsync(roleName))
			{
				await RoleManager.CreateAsync(new IdentityRole(roleName));
			}
		}

		IdentityUser? user = await UserManager.FindByNameAsync("admin");

		if (user is not null) return;

		IdentityUser admin = new()
		{
			UserName = "admin",
			Email = "admin@email.com",
			SecurityStamp = Guid.NewGuid().ToString(),
		};

		string adminPassword = "P@$$w0rd";

		IdentityResult createAdmin = await UserManager.CreateAsync(admin, adminPassword);

		if (createAdmin.Succeeded && !await UserManager.IsInRoleAsync(admin, UserRoles.Admin))
		{
			await UserManager.AddToRoleAsync(admin, UserRoles.Admin);
		}
	}
}
