using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
        var loginPayload = new
        {
            email = "ray@teste.com",
            password = "123456"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(loginPayload),
            Encoding.UTF8, "application/json");

        var login = await _client.PostAsync("/api/auth/login", content);

        if (login.StatusCode != HttpStatusCode.OK)
        {
            var body = await login.Content.ReadAsStringAsync();
            Assert.Fail($"Login falhou: {(int)login.StatusCode} {login.StatusCode}. Body: {body}");
        }

        var loginJson = await login.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(loginJson);
        var token = doc.RootElement.GetProperty("token").GetString();

        token.Should().NotBeNullOrWhiteSpace("o login precisa retornar um token");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var me = await _client.GetAsync("/api/users/me");

        if (me.StatusCode != HttpStatusCode.OK)
        {
            var body = await me.Content.ReadAsStringAsync();
            Assert.Fail($"/me falhou: {(int)me.StatusCode} {me.StatusCode}. Body: {body}");
        }

        var meJson = await me.Content.ReadAsStringAsync();
        using var meDoc = JsonDocument.Parse(meJson);

        var email = meDoc.RootElement.GetProperty("email").GetString();
        var name = meDoc.RootElement.GetProperty("name").GetString();

        email.Should().Be("ray@teste.com");
        name.Should().Be("Raynara Martins");
    }
}
