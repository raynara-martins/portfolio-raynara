using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using PortfolioApi.Models;
using PortfolioApi.Repositories.Interfaces;
using PortfolioApi.Services;

namespace PortfolioApi.Tests.Unit;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepo = null!;
    private IConfiguration _cfg = null!;
    private AuthService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _userRepo = new Mock<IUserRepository>();

        var dict = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "PortfolioApi",
            ["Jwt:Audience"] = "PortfolioFrontend",
            ["Jwt:Secret"] = "CHANGE_ME_SUPER_SECRET_KEY_FOR_TESTS"
        };

        _cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();

        _sut = new AuthService(_userRepo.Object, _cfg);
    }

    [Test]
    public async Task LoginAsync_ShouldSucceed_WhenPasswordMatches()
    {
        var u = new User
        {
            Id = 1,
            Email = "ray@teste.com",
            Name = "Ray",
            Password = "123456"
        };

        _userRepo.Setup(r => r.GetByEmailAsync(u.Email)).ReturnsAsync(u);

        var (ok, token, user) = await _sut.LoginAsync("ray@teste.com", "123456");

        ok.Should().BeTrue();
        token.Should().NotBeNullOrEmpty();
        user!.Email.Should().Be("ray@teste.com");
    }

    [Test]
    public async Task LoginAsync_ShouldFail_WhenPasswordWrong()
    {
        var u = new User
        {
            Id = 1,
            Email = "ray@teste.com",
            Name = "Ray",
            Password = "123456"
        };
    
        _userRepo.Setup(r => r.GetByEmailAsync(u.Email)).ReturnsAsync(u);

        var (ok, token, user) = await _sut.LoginAsync("ray@teste.com", "errada");

        ok.Should().BeFalse();
        token.Should().BeNullOrEmpty();
        user.Should().BeNull();
    }
}
