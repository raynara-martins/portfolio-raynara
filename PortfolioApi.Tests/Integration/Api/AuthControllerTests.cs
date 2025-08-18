using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;

namespace PortfolioApi.Tests.Integration.Api;

[TestFixture]
public class AuthControllerTests
{
    private CustomWebAppFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebAppFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void Teardown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Login_Then_Me_Should_Return200_AndUser()
    {
        // Arrange
        var loginPayload = new { email = "ray@teste.com", password = "123456" };

        // Act 
        var login = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

        if (login.StatusCode != HttpStatusCode.OK)
        {
            var bodyFail = await login.Content.ReadAsStringAsync();
            Assert.Fail($"Login falhou: {(int)login.StatusCode} {login.StatusCode}. Body: {bodyFail}");
        }

        var loginJson = await login.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(loginJson);
        var token = doc.RootElement.GetProperty("token").GetString();

        token.Should().NotBeNullOrWhiteSpace("o login precisa retornar um token válido");

        // Act
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var me = await _client.GetAsync("/api/users/me");

        if (me.StatusCode != HttpStatusCode.OK)
        {
            var bodyFail = await me.Content.ReadAsStringAsync();
            Assert.Fail($"/me falhou: {(int)me.StatusCode} {me.StatusCode}. Body: {bodyFail}");
        }

        // Assert
        var meJson = await me.Content.ReadAsStringAsync();
        using var meDoc = JsonDocument.Parse(meJson);
        meDoc.RootElement.GetProperty("email").GetString().Should().Be("ray@teste.com");
        meDoc.RootElement.GetProperty("name").GetString().Should().Be("Raynara Martins");
    }

    [Test]
    public async Task Login_InvalidPassword_ShouldReturn401()
    {
        var payload = new { email = "ray@teste.com", password = "senha_errada" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            $"Esperava 401, mas veio {(int)response.StatusCode}. Body: {body}");
    }

    [Test]
    public async Task Login_UnknownEmail_ShouldReturn401()
    {
        var payload = new { email = "emaildesconhecido@teste.com", password = "123456" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            $"Esperava 401, mas veio {(int)response.StatusCode}. Body: {body}");
    }

    [Test]
    public async Task Login_MissingFields_ShouldReturn400_FromValidator()
    {
        var payload = new { email = "", password = "" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            $"Esperava 400 (validação), mas veio {(int)response.StatusCode}. Body: {body}");
    }
}
