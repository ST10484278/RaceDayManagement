using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrolmentsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // POST /api/enrolments
    [HttpPost]
    public async Task<ActionResult<EnrolmentDto>> CreateEnrolment(
        [FromBody] CreateEnrolmentRequest request,
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Participant", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Participants can enroll in events.");
        }

        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category == null) return BadRequest("Invalid category ID.");

        var existingEnrolment = await _context.Enrolments
            .AnyAsync(e => e.ParticipantID == userId && e.EventCategoryID == request.CategoryId);

        if (existingEnrolment) return BadRequest("You are already enrolled in this event category.");

        var enrolment = new Enrolment
        {
            ParticipantID = userId,
            EventCategoryID = request.CategoryId,
            EnrollmentStatus = "Confirmed",
            EnrollmentDate = DateTime.UtcNow
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        var dto = new EnrolmentDto(
            enrolment.EnrollmentID,
            enrolment.ParticipantID.ToString(),
            enrolment.EventCategoryID,
            enrolment.EnrollmentStatus,
            enrolment.EnrollmentDate
        );

        return CreatedAtAction(nameof(GetEnrolment), new { id = enrolment.EnrollmentID }, dto);
    }

    // GET /api/enrolments/mine
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<EnrolmentDto>>> GetMyEnrolments(
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Participant", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Participants can view their enrolments.");
        }

        var enrolments = await _context.Enrolments
            .Where(e => e.ParticipantID == userId)
            .Select(e => new EnrolmentDto(
                e.EnrollmentID,
                e.ParticipantID.ToString(),
                e.EventCategoryID,
                e.EnrollmentStatus,
                e.EnrollmentDate
            ))
            .ToListAsync();

        return Ok(enrolments);
    }

    // GET /api/enrolments/event/{eventId} - Organiser views participants for an event
    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<IEnumerable<EnrolmentDto>>> GetEventEnrolments(
        int eventId,
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can view event enrolments.");
        }

        var raceEvent = await _context.Events.FindAsync(eventId);
        if (raceEvent == null) return NotFound("Event not found.");

        if (raceEvent.OrganiserID != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: You can only view enrolments for your own events.");
        }

        var enrolments = await _context.Enrolments
            .Include(e => e.Category)
            .Where(e => e.Category.EventID == eventId)
            .Select(e => new EnrolmentDto(
                e.EnrollmentID,
                e.ParticipantID.ToString(),
                e.EventCategoryID,
                e.EnrollmentStatus,
                e.EnrollmentDate
            ))
            .ToListAsync();

        return Ok(enrolments);
    }

    // PUT /api/enrolments/{id}/status - Organiser manages enrolment status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateEnrolmentStatus(
        int id,
        [FromBody] UpdateEnrolmentStatusRequest request,
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can manage enrolment statuses.");
        }

        var enrolment = await _context.Enrolments
            .Include(e => e.Category)
            .ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(e => e.EnrollmentID == id);

        if (enrolment == null) return NotFound("Enrolment not found.");

        if (enrolment.Category.Event.OrganiserID != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: You can only manage enrolments for your own events.");
        }

        enrolment.EnrollmentStatus = request.Status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET /api/enrolments/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<EnrolmentDto>> GetEnrolment(int id)
    {
        var e = await _context.Enrolments.FindAsync(id);
        if (e == null) return NotFound("Enrolment not found.");

        return Ok(new EnrolmentDto(
            e.EnrollmentID,
            e.ParticipantID.ToString(),
            e.EventCategoryID,
            e.EnrollmentStatus,
            e.EnrollmentDate
        ));
    }
}