using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public CategoriesController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET /api/events/{eventId}/categories - Both roles may view categories for an event
    [HttpGet("events/{eventId}/categories")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategoriesByEvent(int eventId)
    {
        var raceEvent = await _context.Events.FindAsync(eventId);
        if (raceEvent == null)
        {
            return NotFound("Event not found.");
        }

        var categories = await _context.Categories
            .Where(c => c.EventID == eventId)
            .Select(c => new CategoryDto(
                c.EventCategoryID,
                c.EventID,
                c.CategoryName,
                c.Distance
            ))
            .ToListAsync();

        return Ok(categories);
    }

    // POST /api/events/{eventId}/categories - Organisers only (must own parent event)
    [HttpPost("events/{eventId}/categories")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        int eventId,
        [FromBody] CreateCategoryRequest request,
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can add categories.");
        }

        var raceEvent = await _context.Events.FindAsync(eventId);
        if (raceEvent == null)
        {
            return NotFound("Event not found.");
        }

        if (raceEvent.OrganiserID != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: You can only add categories to your own events.");
        }

        var category = new Category
        {
            EventID = eventId,
            CategoryName = request.Name,
            Distance = request.DistanceKm
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var dto = new CategoryDto(
            category.EventCategoryID,
            category.EventID,
            category.CategoryName,
            category.Distance
        );

        return CreatedAtAction(nameof(GetCategoriesByEvent), new { eventId = category.EventID }, dto);
    }

    // PUT /api/categories/{id} - Organisers only (must own parent event)
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        [FromBody] CreateCategoryRequest request,
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can update categories.");
        }

        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.EventCategoryID == id);

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        if (category.Event.OrganiserID != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: You can only update categories for your own events.");
        }

        category.CategoryName = request.Name;
        category.Distance = request.DistanceKm;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/categories/{id} - Organisers only (must own parent event)
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(
        int id,
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can delete categories.");
        }

        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.EventCategoryID == id);

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        if (category.Event.OrganiserID != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: You can only delete categories for your own events.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}