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

    // GET /api/events - Both Organisers and Participants
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
    {
        var events = await _context.Events
            .Select(e => new EventDto(
                e.EventID,
                string.IsNullOrWhiteSpace(e.Title) ? e.EventName : e.Title,
                e.Description,
                e.EventDate,
                string.Empty, // Location property fallback if not present in model
                e.OrganiserID.ToString()
            ))
            .ToListAsync();

        return Ok(events);
    }

    // GET /api/events/{id} - Both Organisers and Participants
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
            string.Empty,
            e.OrganiserID.ToString()
        ));
    }

    // POST /api/events - Organiser ONLY
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
            OrganiserID = userId
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        var dto = new EventDto(
            newEvent.EventID,
            newEvent.Title,
            newEvent.Description,
            newEvent.EventDate,
            request.Location,
            newEvent.OrganiserID.ToString()
        );

        return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventID }, dto);
    }

    // PUT /api/events/{id} - Organiser ONLY
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] UpdateEventRequest request,
        [FromHeader(Name = "X-User-Role")] string userRole)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can edit events.");
        }

        var e = await _context.Events.FindAsync(id);
        if (e == null) return NotFound("Event not found.");

        e.Title = request.Title;
        e.EventName = request.Title;
        e.Description = request.Description;
        e.EventDate = request.Date;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}