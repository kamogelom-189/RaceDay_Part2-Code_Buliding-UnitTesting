
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {
            _context = context;
        }

        // POST: api/events/1/enrolments
        // Participants enter an event by choosing a category.
        [HttpPost("events/{eventId:int}/enrolments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrolment(
            int eventId, EnrolmentDto request)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int participantId))
            {
                return Unauthorized();
            }

            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == request.CategoryId &&
                    c.EventId == eventId);

            if (category == null)
            {
                return BadRequest(new
                {
                    message =
                        "The selected category does not belong to this event."
                });
            }

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantId == participantId &&
                    e.CategoryId == request.CategoryId);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message =
                        "You have already enrolled in this category."
                });
            }

            var enrolment = new Enrolment
            {
                ParticipantId = participantId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.UtcNow,
                PaymentStatus = "Pending"
            };

            _context.Enrolments.Add(enrolment);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment successful.",
                enrolment.EnrolmentId,
                enrolment.ParticipantId,
                enrolment.CategoryId,
                enrolment.EnrolmentDate,
                enrolment.PaymentStatus
            });
        }

        // GET: api/events/1/enrolments
        // Only the organiser who owns the event can view its enrolments.
        [HttpGet("events/{eventId:int}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int organiserId))
            {
                return Unauthorized();
            }

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

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

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.Category.EventId == eventId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    ParticipantName = e.Participant!.FullName,
                    ParticipantEmail = e.Participant.Email,
                    e.CategoryId,
                    CategoryName = e.Category.CategoryName,
                    e.EnrolmentDate,
                    e.PaymentStatus
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/enrolments/my
        // Participants can view only their own enrolments.
        [HttpGet("enrolments/my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int participantId))
            {
                return Unauthorized();
            }

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.ParticipantId == participantId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.CategoryId,
                    CategoryName = e.Category.CategoryName,
                    EventId = e.Category.EventId,
                    EventName = e.Category.Event!.Title,
                    EventDate = e.Category.Event.EventDate,
                    EventLocation = e.Category.Event.Location,
                    e.EnrolmentDate,
                    e.PaymentStatus
                })
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}
