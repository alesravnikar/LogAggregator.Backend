using LogAggregator.Backend;
using LogAggregator.Backend.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
	swagger.SwaggerDoc("v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "ASP.NET 6 Web API",
		Description = "Authentication and Authorization in ASP.NET 6 with JWT and Swagger"
	});

	swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\n Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
	});

	swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});

builder.Services.AddSpaStaticFiles(configuration =>
{
	configuration.RootPath = "wwwroot";
});

builder.Services
	.AddDbContext<UserDbContext>(options => options.UseInMemoryDatabase("UserDb"))
	.AddDbContext<LogDbContext>(options => options.UseInMemoryDatabase("LogDb"));

builder.Services
	.AddIdentity<IdentityUser, IdentityRole>()
	.AddEntityFrameworkStores<UserDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddTransient<DataSeeder>();

builder.Services
	.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.SaveToken = true;
		options.RequireHttpsMetadata = false;
		options.TokenValidationParameters = new TokenValidationParameters()
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidAudience = builder.Configuration["JWT:ValidAudience"],
			ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
		};
	});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// order is important
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseSpa(spa =>
{
	spa.Options.SourcePath = "wwwroot";
});

using (IServiceScope scope = app.Services.CreateScope())
{
	// todo: this is meant as a database seed (populate roles and create admin)
	// but it's breaking tests and I'm not sure how to fix it
	DataSeeder seeder = scope.ServiceProvider.GetService<DataSeeder>()!;
	await seeder.Seed();
}

app.Run();

public partial class Program
{

}