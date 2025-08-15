using PortfolioApi.Models;

namespace Tests.Common.Builders;

public class UserBuilder
{
    private readonly User _u = new()
    {
        Id = 1,
        Name = "Ray",
        Email = "ray@teste.com",
        Password = "123456"
    };

    public UserBuilder WithEmail(string email) { _u.Email = email; return this; }
    public UserBuilder WithHashedPassword(string plain)
    {
        _u.Password = BCrypt.Net.BCrypt.HashPassword(plain);
        return this;
    }

    public User Build() => _u;
}
