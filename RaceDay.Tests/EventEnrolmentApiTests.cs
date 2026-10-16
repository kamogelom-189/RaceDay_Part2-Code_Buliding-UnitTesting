
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace RaceDay.Tests
{
    public class EventEnrolmentApiTests
    {
        [Fact]
        public async Task Organiser_CanCreateEvent()
        {
            using var factory = new RaceDayWebApplicationFactory();
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();

            var token = await TestApiHelpers.RegisterAndLoginAsync(
                client,
                "Test Organiser",
                "organiser@example.com",
                "Organiser");

            TestApiHelpers.UseToken(client, token);

            int eventId =
                await TestApiHelpers.CreateEventAsync(client);

            Assert.True(eventId > 0);
        }

        [Fact]
        public async Task Participant_CannotCreateEvent()
        {
            using var factory = new RaceDayWebApplicationFactory();
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();

            var token = await TestApiHelpers.RegisterAndLoginAsync(
                client,
                "Test Participant",
                "participant@example.com",
                "Participant");

            TestApiHelpers.UseToken(client, token);

            var response = await client.PostAsJsonAsync(
                "/api/events",
                new
                {
                    title = "Unauthorized Event",
                    description = "This must be rejected",
                    eventDate = DateTime.UtcNow.AddDays(30),
                    location = "Johannesburg",
                    distanceKM = 10m,
                    eventType = "run"
                });

            await AssertStatusAsync(
                response,
                HttpStatusCode.Forbidden,
                "Participant should not create an event");
        }

        [Fact]
        public async Task Events_ReturnsUnauthorized_WithoutToken()
        {
            using var factory = new RaceDayWebApplicationFactory();
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/events");

            await AssertStatusAsync(
                response,
                HttpStatusCode.Unauthorized,
                "Viewing events without a token");
        }

        [Fact]
        public async Task Organiser_CannotUpdateAnotherOrganisersEvent()
        {
            using var factory = new RaceDayWebApplicationFactory();
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();

            var ownerToken =
                await TestApiHelpers.RegisterAndLoginAsync(
                    client,
                    "First Organiser",
                    "firstorganiser@example.com",
                    "Organiser");

            TestApiHelpers.UseToken(client, ownerToken);

            int eventId =
                await TestApiHelpers.CreateEventAsync(client);

            var otherToken =
                await TestApiHelpers.RegisterAndLoginAsync(
                    client,
                    "Second Organiser",
                    "secondorganiser@example.com",
                    "Organiser");

            TestApiHelpers.UseToken(client, otherToken);

            var response = await client.PutAsJsonAsync(
                $"/api/events/{eventId}",
                new
                {
                    title = "Attempted Unauthorized Update",
                    description = "This update must be rejected",
                    eventDate = DateTime.UtcNow.AddDays(60),
                    location = "Johannesburg",
                    distanceKM = 15m,
                    eventType = "run"
                });

            await AssertStatusAsync(
                response,
                HttpStatusCode.Forbidden,
                "Updating another organiser's event");
        }

        [Fact]
        public async Task Participant_CannotCreateCategory()
        {
            using var factory = new RaceDayWebApplicationFactory();
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();

            var organiserToken =
                await TestApiHelpers.RegisterAndLoginAsync(
                    client,
                    "Test Organiser",
                    "categoryorganiser@example.com",
                    "Organiser");

            TestApiHelpers.UseToken(client, organiserToken);

            int eventId =
                await TestApiHelpers.CreateEventAsync(client);

            var participantToken =
                await TestApiHelpers.RegisterAndLoginAsync(
                    client,
                    "Test Participant",
                    "categoryparticipant@example.com",
                    "Participant");

            TestApiHelpers.UseToken(client, participantToken);

            var response = await client.PostAsJsonAsync(
                $"/api/events/{eventId}/categories",
                new
                {
                    categoryName = "Unauthorized Category",
                    distanceKM = 10m,
                    entryFee = 150m
                });

            await AssertStatusAsync(
                response,
                HttpStatusCode.Forbidden,
                "Participant should not create a category");
        }

        [Fact]
        public async Task Participant_CanEnrolOnce_AndViewOwnEnrolments()
        {
            using var factory = new RaceDayWebApplicationFactory();
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();

            var organiserToken =
                await TestApiHelpers.RegisterAndLoginAsync(
                    client,
                    "Test Organiser",
                    "enrolorganiser@example.com",
                    "Organiser");

            TestApiHelpers.UseToken(client, organiserToken);

            int eventId =
                await TestApiHelpers.CreateEventAsync(client);

            int categoryId =
                await TestApiHelpers.CreateCategoryAsync(
                    client, eventId);

            var participantToken =
                await TestApiHelpers.RegisterAndLoginAsync(
                    client,
                    "Test Participant",
                    "enrolparticipant@example.com",
                    "Participant");

            TestApiHelpers.UseToken(client, participantToken);

            int enrolmentId =
                await TestApiHelpers.CreateEnrolmentAsync(
                    client, eventId, categoryId);

            Assert.True(enrolmentId > 0);

            // The same participant cannot enrol twice
            // in the same category.
            var duplicateResponse = await client.PostAsJsonAsync(
                $"/api/events/{eventId}/enrolments",
                new { categoryId });

            await AssertStatusAsync(
                duplicateResponse,
                HttpStatusCode.Conflict,
                "Duplicate enrolment");

            // Participants can view their own enrolments.
            var myEnrolmentsResponse =
                await client.GetAsync("/api/enrolments/my");

            await AssertStatusAsync(
                myEnrolmentsResponse,
                HttpStatusCode.OK,
                "Viewing the participant's own enrolments");

            // Organisers can view enrolments for their events.
            TestApiHelpers.UseToken(client, organiserToken);

            var eventEnrolmentsResponse =
                await client.GetAsync(
                    $"/api/events/{eventId}/enrolments");

            await AssertStatusAsync(
                eventEnrolmentsResponse,
                HttpStatusCode.OK,
                "Organiser viewing event enrolments");
        }

        [Fact]
        public async Task Organiser_CanRecordResult_AndParticipantCanViewIt()
        {
            using var factory = new RaceDayWebApplicationFactory();
            await factory.InitializeDatabaseAsync();
            using var client = factory.CreateClient();

            // Register and log in as the event organiser.
            var organiserToken =
                await TestApiHelpers.RegisterAndLoginAsync(
                    client,
                    "Test Organiser",
                    "resultorganiser@example.com",
                    "Organiser");

            TestApiHelpers.UseToken(client, organiserToken);

            // Create the event and category.
            int eventId =
                await TestApiHelpers.CreateEventAsync(client);

            int categoryId =
                await TestApiHelpers.CreateCategoryAsync(
                    client, eventId);

            // Register a participant and enrol them.
            var participantToken =
                await TestApiHelpers.RegisterAndLoginAsync(
                    client,
                    "Test Participant",
                    "resultparticipant@example.com",
                    "Participant");

            TestApiHelpers.UseToken(client, participantToken);

            int enrolmentId =
                await TestApiHelpers.CreateEnrolmentAsync(
                    client, eventId, categoryId);

            Assert.True(enrolmentId > 0);

            // Return to the organiser account to record the result.
            TestApiHelpers.UseToken(client, organiserToken);

            var resultResponse = await client.PostAsJsonAsync(
                $"/api/events/{eventId}/results",
                new
                {
                    enrolmentId,
                    raceNumber = "RD001",
                    finishTime = "00:45:12",
                    position = 1
                });

            await AssertStatusAsync(
                resultResponse,
                HttpStatusCode.OK,
                "Recording a result");

            // Organiser views results for the event.
            var eventResultsResponse =
                await client.GetAsync(
                    $"/api/events/{eventId}/results");

            await AssertStatusAsync(
                eventResultsResponse,
                HttpStatusCode.OK,
                "Organiser viewing event results");

            // Participant views their own result.
            TestApiHelpers.UseToken(client, participantToken);

            var myResultsResponse =
                await client.GetAsync("/api/results/my");

            await AssertStatusAsync(
                myResultsResponse,
                HttpStatusCode.OK,
                "Participant viewing their own results");
        }

        // Provides useful details when a test gets the wrong status code.
        private static async Task AssertStatusAsync(
            HttpResponseMessage response,
            HttpStatusCode expectedStatus,
            string operation)
        {
            string responseBody =
                await response.Content.ReadAsStringAsync();

            Assert.True(
                response.StatusCode == expectedStatus,
                $"{operation} failed. " +
                $"Expected: {(int)expectedStatus} {expectedStatus}. " +
                $"Actual: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response body: {responseBody}");
        }
    }
}
