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

    // POST /api/enrolments - Participant ONLY
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

        var enrolment = new Enrolment
        {
            ParticipantID = userId,
            EventCategoryID = request.CategoryId,
            EnrollmentStatus = "Registered",
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