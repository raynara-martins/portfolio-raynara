using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortfolioApi.Data;

namespace PortfolioApi.Tests.Integration;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"]   = "PortfolioApi",
                ["Jwt:Audience"] = "PortfolioFrontend",
                ["Jwt:Secret"]   = "THIS_IS_A_VERY_LONG_TEST_SECRET_KEY_32+_CHARS_!!"
            };
            cfg.AddInMemoryCollection(overrides);
        });

        builder.ConfigureServices(services =>
        {
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                var hashed = BCrypt.Net.BCrypt.HashPassword("123456");
                db.Users.Add(new PortfolioApi.Models.User
                {
                    Name = "Raynara Martins",
                    Email = "ray@teste.com",
                    Password = hashed
                });
                db.SaveChanges();
            }
        });
    }
}
