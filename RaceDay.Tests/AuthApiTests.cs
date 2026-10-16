
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthApiTests
    {
        [Fact]
        public async Task Register_ReturnsSuccess_WhenDetailsAreValid()
        {
            using var factory =
                new RaceDayWebApplicationFactory();

            await factory.InitializeDatabaseAsync();

            using var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    fullName = "Test Participant",
                    email = "participant1@example.com",
                    password = "StrongPass123!",
                    roleName = "Participant"
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
        {
            using var factory =
                new RaceDayWebApplicationFactory();

            await factory.InitializeDatabaseAsync();

            using var client = factory.CreateClient();

            var request = new
            {
                fullName = "Test Participant",
                email = "duplicate@example.com",
                password = "StrongPass123!",
                roleName = "Participant"
            };

            await client.PostAsJsonAsync(
                "/api/Auth/register", request);

            var response = await client.PostAsJsonAsync(
                "/api/Auth/register", request);

            Assert.Equal(
                HttpStatusCode.Conflict,
                response.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsToken_WhenCredentialsAreCorrect()
        {
            using var factory =
                new RaceDayWebApplicationFactory();

            await factory.InitializeDatabaseAsync();

            using var client = factory.CreateClient();

            await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    fullName = "Login Tester",
                    email = "login@example.com",
                    password = "StrongPass123!",
                    roleName = "Participant"
                });

            var response = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "login@example.com",
                    password = "StrongPass123!"
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result =
                await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.Equal("Participant", result.Role);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
        {
            using var factory =
                new RaceDayWebApplicationFactory();

            await factory.InitializeDatabaseAsync();

            using var client = factory.CreateClient();

            await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    fullName = "Login Tester",
                    email = "wrongpassword@example.com",
                    password = "StrongPass123!",
                    roleName = "Participant"
                });

            var response = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "wrongpassword@example.com",
                    password = "WrongPassword123!"
                });

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
    }
}
