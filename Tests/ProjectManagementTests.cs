using LogAggregator.Backend.Database.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Xunit;
// hack: I don't know how to correctly structure tests
using static Tests.UserManagementTests;

namespace Tests;

public class ProjectManagementTests : IClassFixture<WebApplicationFactory<Program>>
{
	public WebApplicationFactory<Program> App { get; }

	public ProjectManagementTests(WebApplicationFactory<Program> app)
	{
		App = app;
	}

	[Fact]
	public async void AdminCanAddProject()
	{
		HttpClient client = App.CreateClient();

		HttpResponseMessage loginAdminResponse = await LoginAdmin(App);
		var token = await GetTokenFromResponse(loginAdminResponse);

		HttpRequestMessage request = new(HttpMethod.Post, "/Project/AddProject")
		{
			Content = JsonContent.Create(new ProjectCreateModel
			{
				Name = "Project 1",
			})
		};

		if (token is not null)
		{
			request.Headers.Add("Authorization", $"Bearer {token.Token}");
		}

		HttpResponseMessage response = await client.SendAsync(request);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async void UserCanAddProject()
	{
		HttpClient client = App.CreateClient();

		await CreateUserAsAdmin(App, User1);

		HttpResponseMessage loginUserResponse = await LoginUser(App, User1);
		LoginResponseModel token = await GetTokenFromResponse(loginUserResponse);

		HttpRequestMessage request = new(HttpMethod.Post, "/Project/AddProject")
		{
			Content = JsonContent.Create(new ProjectCreateModel
			{
				Name = "Project 1",
			})
		};

		if (token is not null)
		{
			request.Headers.Add("Authorization", $"Bearer {token.Token}");
		}

		HttpResponseMessage response = await client.SendAsync(request);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async void CanNotAddProjectWithoutAuthentication()
	{
		HttpClient client = App.CreateClient();

		HttpRequestMessage request = new(HttpMethod.Post, "/Project/AddProject")
		{
			Content = JsonContent.Create(new ProjectCreateModel
			{
				Name = "Project 1",
			})
		};

		HttpResponseMessage response = await client.SendAsync(request);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async void CanNotFetchProjectsWithoutAuthentication()
	{
		HttpClient client = App.CreateClient();

		HttpRequestMessage request = new(HttpMethod.Get, "/Project/GetProjects");

		HttpResponseMessage response = await client.SendAsync(request);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async void CanNotAddLogToProjectWithoutAuthentication()
	{
		HttpClient client = App.CreateClient();

		HttpRequestMessage request = new(HttpMethod.Post, "/Project/AddLogToProject")
		{
			Content = JsonContent.Create(new AddLogToProjectModel())
		};

		HttpResponseMessage response = await client.SendAsync(request);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async void CanNotGetProjectLogsWithoutAuthentication()
	{
		HttpClient client = App.CreateClient();

		HttpRequestMessage request = new(HttpMethod.Get, "/Project/GetProjectLogs");

		HttpResponseMessage response = await client.SendAsync(request);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async void CanNotGetProjectStatisticsWithoutAuthentication()
	{
		HttpClient client = App.CreateClient();

		HttpRequestMessage request = new(HttpMethod.Get, "/Project/GetProjectStatistics");

		HttpResponseMessage response = await client.SendAsync(request);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async void CanNotExportLogsToExcelWithoutAuthentication()
	{
		HttpClient client = App.CreateClient();

		HttpRequestMessage request = new(HttpMethod.Get, "/Project/ExportLogsToExcel");

		HttpResponseMessage response = await client.SendAsync(request);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}
}