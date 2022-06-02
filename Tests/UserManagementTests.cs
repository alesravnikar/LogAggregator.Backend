using LogAggregator.Backend.Database.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Tests;

public class UserManagementTests : IClassFixture<WebApplicationFactory<Program>>
{
	public WebApplicationFactory<Program> App { get; }

	public static RegisterModel User1 { get; } = new()
	{
		Username = "user",
		Password = "P@$$w0rd",
		Email = "user@user.com",
	};

	private static RegisterModel User2 { get; } = new()
	{
		Username = "user2",
		Password = "P@$$w0rd",
		Email = "user2@user.com",
	};

	public UserManagementTests(WebApplicationFactory<Program> app)
	{
		App = app;
	}

	public static async Task<HttpResponseMessage> LoginAdmin(WebApplicationFactory<Program> app)
	{
		HttpClient client = app.CreateClient();

		JsonContent content = JsonContent.Create(new LoginModel
		{
			Username = "admin",
			Password = "P@$$w0rd"
		});

		return await client.PostAsync("/User/Login", content);
	}

	public static async Task<LoginResponseModel> GetTokenFromResponse(HttpResponseMessage response)
	{
		string tokenString = await response.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<LoginResponseModel>(tokenString)
			?? throw new Exception("Failed to get token from response.");
	}

	public static async Task<HttpResponseMessage> CreateUserAsAdmin
	(
		WebApplicationFactory<Program> app,
		RegisterModel user
	)
	{
		HttpResponseMessage adminLoginResponse = await LoginAdmin(app);

		LoginResponseModel token = await GetTokenFromResponse(adminLoginResponse);

		return await CreateUser(app, user, token);
	}

	public static async Task<HttpResponseMessage> LoginUser(WebApplicationFactory<Program> app, RegisterModel user)
	{
		HttpClient client = app.CreateClient();

		JsonContent content = JsonContent.Create(user);

		return await client.PostAsync("/User/Login", content);
	}

	private static async Task<HttpResponseMessage> CreateUser
	(
		WebApplicationFactory<Program> app,
		RegisterModel user,
		LoginResponseModel? token = null
	)
	{
		HttpClient client = app.CreateClient();

		HttpRequestMessage request = new(HttpMethod.Post, "/User/Register")
		{
			Content = JsonContent.Create(User1)
		};

		if (token is not null)
		{
			request.Headers.Add("Authorization", $"Bearer {token.Token}");
		}

		return await client.SendAsync(request);
	}

	[Fact]
	public async Task AdminLoginWorks()
	{
		HttpResponseMessage response = await LoginAdmin(App);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task CanNotCreateUserWithoutAuthentication()
	{
		HttpResponseMessage response = await CreateUser(App, User1);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task AdminCanCreateUser()
	{
		HttpResponseMessage response = await CreateUserAsAdmin(App, User1);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task UserCanNotCreateUser()
	{
		await CreateUserAsAdmin(App, User1);

		HttpResponseMessage loginResponse = await LoginUser(App, User1);
		LoginResponseModel token = await GetTokenFromResponse(loginResponse);

		HttpResponseMessage response = await CreateUser(App, User2, token);

		Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
	}
}
