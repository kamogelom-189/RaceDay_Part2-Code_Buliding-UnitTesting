
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Api.Data;

namespace RaceDay.Tests
{
    public class RaceDayWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        private const string TestJwtKey =
            "RaceDay_Test_Key_At_Least_32_Characters_Long_123456!";

        private const string TestIssuer = "RaceDay.Api";

        private const string TestAudience = "RaceDay.Client";

        private readonly string _databaseName =
            "RaceDayTests_" + Guid.NewGuid().ToString("N");

        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Jwt:Key"] = TestJwtKey,
                        ["Jwt:Issuer"] = TestIssuer,
                        ["Jwt:Audience"] = TestAudience
                    });
            });

            builder.ConfigureServices(services =>
            {
                // Remove the production database registrations.
                services.RemoveAll<DbContextOptions<RaceDayContext>>();
                services.RemoveAll<RaceDayContext>();

                // Use an isolated database for this test factory.
                services.AddDbContext<RaceDayContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                // Make JWT validation use the same settings
                // as the test login endpoint.
                services.PostConfigure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,

                                ValidIssuer = TestIssuer,
                                ValidAudience = TestAudience,

                                IssuerSigningKey =
                                    new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(TestJwtKey))
                            };
                    });
            });
        }

        public async Task InitializeDatabaseAsync()
        {
            using var scope = Services.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<RaceDayContext>();

            await context.Database.EnsureCreatedAsync();
        }
    }
}
