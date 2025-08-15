using FluentAssertions;
using NUnit.Framework;
using PortfolioApi.DTOs.Auth;
using PortfolioApi.Validators;

namespace PortfolioApi.Tests.Unit;

[TestFixture]
public class LoginRequestValidatorTests
{
    [Test]
    public void Should_Fail_When_Email_Empty()
    {
        var v = new LoginRequestValidator();
        var res = v.Validate(new LoginRequest { Email = "", Password = "123456" });

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Test]
    public void Should_Fail_When_Password_Short()
    {
        var v = new LoginRequestValidator();
        var res = v.Validate(new LoginRequest { Email = "ray@teste.com", Password = "123" });

        res.IsValid.Should().BeFalse();
        res.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Test]
    public void Should_Pass_When_Valid()
    {
        var v = new LoginRequestValidator();
        var res = v.Validate(new LoginRequest { Email = "ray@teste.com", Password = "123456" });

        res.IsValid.Should().BeTrue();
    }
}
