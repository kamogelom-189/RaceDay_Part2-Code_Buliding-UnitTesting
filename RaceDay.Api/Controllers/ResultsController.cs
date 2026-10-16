
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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        // POST: /api/events/1/results
        // Only the organiser who owns the event can record results.
        [HttpPost("events/{eventId:int}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(
            int eventId, CreateResultDto request)
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

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == request.EnrolmentId &&
                    e.Category.EventId == eventId);

            if (enrolment == null)
            {
                return BadRequest(new
                {
                    message =
                        "The enrolment does not belong to this event."
                });
            }

            bool resultAlreadyExists = await _context.Results
                .AnyAsync(r =>
                    r.EnrolmentId == request.EnrolmentId);

            if (resultAlreadyExists)
            {
                return Conflict(new
                {
                    message =
                        "A result already exists for this enrolment."
                });
            }

            if (request.FinishTime.HasValue &&
                request.FinishTime.Value < TimeSpan.Zero)
            {
                return BadRequest(new
                {
                    message = "Finish time cannot be negative."
                });
            }

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                RaceNumber = request.RaceNumber.Trim(),
                FinishTime = request.FinishTime,
                Position = request.Position
            };

            _context.Results.Add(result);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result recorded successfully.",
                result.ResultId,
                result.EnrolmentId,
                result.RaceNumber,
                result.FinishTime,
                result.Position
            });
        }

        // GET: /api/events/1/results
        // An organiser views results for their own event.
        [HttpGet("events/{eventId:int}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventResults(int eventId)
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

            var results = await _context.Results
                .AsNoTracking()
                .Where(r => r.Enrolment.Category.EventId == eventId)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    r.RaceNumber,
                    r.FinishTime,
                    r.Position,
                    r.Enrolment.ParticipantId,
                    ParticipantName =
                        r.Enrolment.Participant!.FullName,
                    CategoryName =
                        r.Enrolment.Category.CategoryName
                })
                .OrderBy(r => r.Position)
                .ToListAsync();

            return Ok(results);
        }

        // GET: /api/results/my
        // Participants can view only their own results.
        [HttpGet("results/my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int participantId))
            {
                return Unauthorized();
            }

            var results = await _context.Results
                .AsNoTracking()
                .Where(r =>
                    r.Enrolment.ParticipantId == participantId)
                .Select(r => new
                {
                    r.ResultId,
                    r.RaceNumber,
                    r.FinishTime,
                    r.Position,
                    CategoryName =
                        r.Enrolment.Category.CategoryName,
                    EventId =
                        r.Enrolment.Category.EventId,
                    EventName =
                        r.Enrolment.Category.Event!.Title,
                    EventDate =
                        r.Enrolment.Category.Event.EventDate
                })
                .OrderBy(r => r.EventDate)
                .ToListAsync();

            return Ok(results);
        }
    }
}
