
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
    [Route("api/events/{eventId:int}/categories")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/events/1/categories
        // Both roles can view categories.
        [HttpGet]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.EventId == eventId)
                .OrderBy(c => c.CategoryName)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.CategoryName,
                    c.DistanceKM,
                    c.EntryFee
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/events/1/categories/2
        [HttpGet("{categoryId:int}")]
        public async Task<IActionResult> GetCategory(
            int eventId, int categoryId)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.EventId == eventId
                         && c.CategoryId == categoryId)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.CategoryName,
                    c.DistanceKM,
                    c.EntryFee
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            return Ok(category);
        }

        // POST: api/events/1/categories
        // Only the event's Organiser can add a category.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(
            int eventId, CreateCategoryDto request)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int userId))
            {
                return Unauthorized();
            }

            if (eventItem.OrganiserId != userId)
            {
                return Forbid();
            }

            var category = new Category
            {
                EventId = eventId,
                CategoryName = request.CategoryName.Trim(),
                DistanceKM = request.DistanceKM,
                EntryFee = request.EntryFee
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new
                {
                    eventId,
                    categoryId = category.CategoryId
                },
                new
                {
                    message = "Category created successfully.",
                    category.CategoryId,
                    category.EventId,
                    category.CategoryName,
                    category.DistanceKM,
                    category.EntryFee
                });
        }

        // PUT: api/events/1/categories/2
        [HttpPut("{categoryId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int eventId,
            int categoryId,
            CreateCategoryDto request)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == categoryId &&
                    c.EventId == eventId);

            if (category == null || category.Event == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int userId))
            {
                return Unauthorized();
            }

            if (category.Event.OrganiserId != userId)
            {
                return Forbid();
            }

            category.CategoryName = request.CategoryName.Trim();
            category.DistanceKM = request.DistanceKM;
            category.EntryFee = request.EntryFee;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully."
            });
        }

        // DELETE: api/events/1/categories/2
        [HttpDelete("{categoryId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(
            int eventId, int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == categoryId &&
                    c.EventId == eventId);

            if (category == null || category.Event == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int userId))
            {
                return Unauthorized();
            }

            if (category.Event.OrganiserId != userId)
            {
                return Forbid();
            }

            bool hasEnrolments = await _context.Enrolments
                .AnyAsync(e => e.CategoryId == categoryId);

            if (hasEnrolments)
            {
                return Conflict(new
                {
                    message =
                        "Cannot delete a category with existing enrolments."
                });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }
    }
}
