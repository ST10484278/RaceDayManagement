using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET /api/events - Both roles may view all Events
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
    {
        var events = await _context.Events
            .Select(e => new EventDto(
                e.EventID,
                string.IsNullOrWhiteSpace(e.Title) ? e.EventName : e.Title,
                e.Description,
                e.EventDate,
                e.Location,
                e.OrganiserID.ToString()
            ))
            .ToListAsync();

        return Ok(events);
    }

    // GET /api/events/{id} - Both roles may view a single Event
    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var e = await _context.Events.FindAsync(id);
        if (e == null) return NotFound("Event not found.");

        return Ok(new EventDto(
            e.EventID,
            string.IsNullOrWhiteSpace(e.Title) ? e.EventName : e.Title,
            e.Description,
            e.EventDate,
            e.Location,
            e.OrganiserID.ToString()
        ));
    }

    // GET /api/events/mine - Organisers view events they created
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetMyEvents(
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can view their created events.");
        }

        var myEvents = await _context.Events
            .Where(e => e.OrganiserID == userId)
            .Select(e => new EventDto(
                e.EventID,
                string.IsNullOrWhiteSpace(e.Title) ? e.EventName : e.Title,
                e.Description,
                e.EventDate,
                e.Location,
                e.OrganiserID.ToString()
            ))
            .ToListAsync();

        return Ok(myEvents);
    }

    // POST /api/events - Only Organisers may create Events
    [HttpPost]
    public async Task<ActionResult<EventDto>> CreateEvent(
        [FromBody] CreateEventRequest request,
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can create events.");
        }

        var newEvent = new Event
        {
            Title = request.Title,
            EventName = request.Title,
            Description = request.Description,
            EventDate = request.Date,
            Location = request.Location,
            OrganiserID = userId
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        var dto = new EventDto(
            newEvent.EventID,
            newEvent.Title,
            newEvent.Description,
            newEvent.EventDate,
            newEvent.Location,
            newEvent.OrganiserID.ToString()
        );

        return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventID }, dto);
    }

    // PUT /api/events/{id} - Organisers may only update Events they own
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] UpdateEventRequest request,
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can update events.");
        }

        var e = await _context.Events.FindAsync(id);
        if (e == null) return NotFound("Event not found.");

        // Ownership check
        if (e.OrganiserID != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: You can only update events that you created.");
        }

        e.Title = request.Title;
        e.EventName = request.Title;
        e.Description = request.Description;
        e.EventDate = request.Date;
        e.Location = request.Location;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/events/{id} - Organisers may only delete Events they own
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(
        int id,
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can delete events.");
        }

        var e = await _context.Events.FindAsync(id);
        if (e == null) return NotFound("Event not found.");

        // Ownership check
        if (e.OrganiserID != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: You can only delete events that you created.");
        }

        _context.Events.Remove(e);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}