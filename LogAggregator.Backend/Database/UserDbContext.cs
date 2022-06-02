using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LogAggregator.Backend.Database;

public class UserDbContext : IdentityDbContext<IdentityUser>
{
	public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
	{

	}

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
	}
}
