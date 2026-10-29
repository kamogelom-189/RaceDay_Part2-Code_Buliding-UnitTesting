using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using System.Security.Claims;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;
        private readonly IBlobStorageService _blobStorage;

        public EventsController(
            RaceDayContext context,
            IBlobStorageService blobStorage)
        {
            _context = context;
            _blobStorage = blobStorage;
        }

        // GET: api/events
        // Both roles can view events.
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.DistanceKM,
                    e.EventType,
                    e.OrganiserId
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var raceEvent = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.DistanceKM,
                    e.EventType,
                    e.OrganiserId
                })
                .FirstOrDefaultAsync();

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            return Ok(raceEvent);
        }

        // POST: api/events
        // Only Organisers can create events.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(
            EventRequestDto request)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int organiserId))
            {
                return Unauthorized();
            }

            string eventType =
                request.EventType.Trim().ToLowerInvariant();

            if (!new[] { "run", "walk", "cycle" }
                .Contains(eventType))
            {
                return BadRequest(new
                {
                    message = "Event type must be run, walk or cycle."
                });
            }

            var raceEvent = new RaceEvent
            {
                OrganiserId = organiserId,
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                EventDate = request.EventDate!.Value,
                Location = request.Location.Trim(),
                DistanceKM = request.DistanceKM,
                EventType = eventType
            };

            _context.Events.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventById),
                new { id = raceEvent.EventId },
                new
                {
                    raceEvent.EventId,
                    raceEvent.Title,
                    raceEvent.Description,
                    raceEvent.EventDate,
                    raceEvent.Location,
                    raceEvent.DistanceKM,
                    raceEvent.EventType,
                    raceEvent.OrganiserId
                });
        }

        // PUT: api/events/1
        // Organisers can update their own events.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            EventRequestDto request)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int organiserId))
            {
                return Unauthorized();
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (raceEvent.OrganiserId != organiserId)
            {
                return Forbid();
            }

            string eventType =
                request.EventType.Trim().ToLowerInvariant();

            if (!new[] { "run", "walk", "cycle" }
                .Contains(eventType))
            {
                return BadRequest(new
                {
                    message = "Event type must be run, walk or cycle."
                });
            }

            raceEvent.Title = request.Title.Trim();
            raceEvent.Description = request.Description?.Trim();
            raceEvent.EventDate = request.EventDate!.Value;
            raceEvent.Location = request.Location.Trim();
            raceEvent.DistanceKM = request.DistanceKM;
            raceEvent.EventType = eventType;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully."
            });
        }

        // DELETE: api/events/1
        // Organisers can delete their own events.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int organiserId))
            {
                return Unauthorized();
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (raceEvent.OrganiserId != organiserId)
            {
                return Forbid();
            }

            bool hasEnrolments = await _context.Enrolments
                .AnyAsync(e => e.Category != null
                    && e.Category.EventId == id);

            if (hasEnrolments)
            {
                return Conflict(new
                {
                    message = "This event has enrolments and cannot be deleted."
                });
            }

            _context.Events.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }

        // POST: api/events/1/banner
        [HttpPost("{id:int}/banner")]
        [Authorize(Roles = "Organiser")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int organiserId))
                return Unauthorized();

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.OrganiserId != organiserId)
                return Forbid();

            string? error = ImageValidator.Validate(file);
            if (error != null)
                return BadRequest(new { message = error });

            await using var stream = file.OpenReadStream();
            raceEvent.BannerUrl = await _blobStorage.UploadAsync(
                stream, file.FileName, file.ContentType);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Banner uploaded successfully.",
                raceEvent.BannerUrl
            });
        }
    }
}