
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Api.DTOs;
using Xunit;

namespace RaceDay.Tests
{
    public static class TestApiHelpers
    {
        public static async Task<string> RegisterAndLoginAsync(
            HttpClient client,
            string fullName,
            string email,
            string roleName)
        {
            const string password = "StrongPass123!";

            var registerResponse = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    fullName,
                    email,
                    password,
                    roleName
                });

            Assert.Equal(
                HttpStatusCode.OK,
                registerResponse.StatusCode);

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new { email, password });

            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var result =
                await loginResponse.Content
                    .ReadFromJsonAsync<AuthResponseDto>();

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));

            return result.Token;
        }

        public static void UseToken(
            HttpClient client,
            string token)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public static async Task<int> CreateEventAsync(
            HttpClient client,
            string title = "Test RaceDay Run")
        {
            var response = await client.PostAsJsonAsync(
                "/api/events",
                new
                {
                    title,
                    description = "Automated test event",
                    eventDate = DateTime.UtcNow.AddDays(30),
                    location = "Johannesburg",
                    distanceKM = 10m,
                    eventType = "run"
                });

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            return await ReadIdAsync(response, "eventId");
        }

        public static async Task<int> CreateCategoryAsync(
            HttpClient client,
            int eventId)
        {
            var response = await client.PostAsJsonAsync(
                $"/api/events/{eventId}/categories",
                new
                {
                    categoryName = "Test 10km",
                    distanceKM = 10m,
                    entryFee = 150m
                });

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode);

            return await ReadIdAsync(response, "categoryId");
        }

        public static async Task<int> CreateEnrolmentAsync(
            HttpClient client,
            int eventId,
            int categoryId)
        {
            var response = await client.PostAsJsonAsync(
                $"/api/events/{eventId}/enrolments",
                new { categoryId });

            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode);

            return await ReadIdAsync(response, "enrolmentId");
        }

        public static async Task<int> ReadIdAsync(
            HttpResponseMessage response,
            string propertyName)
        {
            string json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            return document.RootElement
                .GetProperty(propertyName)
                .GetInt32();
        }
    }
}
