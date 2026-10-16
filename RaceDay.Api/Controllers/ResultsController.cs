using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // POST /api/results - Organiser ONLY (Records finish time and position)
    [HttpPost]
    public async Task<ActionResult<ResultDto>> CreateResult(
        [FromBody] CreateResultRequest request,
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Organisers can enter race results.");
        }

        // Verify enrolment exists
        var enrolment = await _context.Enrolments
            .Include(e => e.Category)
            .ThenInclude(c => c.Event)
            .FirstOrDefaultAsync(e => e.EnrollmentID == request.EnrolmentId);

        if (enrolment == null)
        {
            return NotFound("Enrolment record not found.");
        }

        // Verify event ownership
        if (enrolment.Category.Event.OrganiserID != userId)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: You can only post results for your own events.");
        }

        var result = new Result
        {
            EnrollmentID = request.EnrolmentId,
            FinishTime = request.CompletionTime,
            Position = request.Rank
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        var dto = new ResultDto(
            result.ResultID,
            result.EnrollmentID,
            result.FinishTime,
            result.Position
        );

        return CreatedAtAction(nameof(GetResultById), new { id = result.ResultID }, dto);
    }

    // GET /api/results/mine - Participant ONLY
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<ResultDto>>> GetMyResults(
        [FromHeader(Name = "X-User-Role")] string userRole,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (!string.Equals(userRole, "Participant", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Access Denied: Only Participants can view their results.");
        }

        var results = await _context.Results
            .Include(r => r.Enrolment)
            .Where(r => r.Enrolment.ParticipantID == userId)
            .Select(r => new ResultDto(
                r.ResultID,
                r.EnrollmentID,
                r.FinishTime,
                r.Position
            ))
            .ToListAsync();

        return Ok(results);
    }

    // GET /api/results/event/{eventId}
    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<IEnumerable<ResultDto>>> GetEventResults(int eventId)
    {
        var raceEvent = await _context.Events.FindAsync(eventId);
        if (raceEvent == null)
        {
            return NotFound("Event not found.");
        }

        var results = await _context.Results
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.Category)
            .Where(r => r.Enrolment.Category.EventID == eventId)
            .OrderBy(r => r.Position)
            .Select(r => new ResultDto(
                r.ResultID,
                r.EnrollmentID,
                r.FinishTime,
                r.Position
            ))
            .ToListAsync();

        return Ok(results);
    }

    // GET /api/results/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ResultDto>> GetResultById(int id)
    {
        var r = await _context.Results.FindAsync(id);
        if (r == null) return NotFound("Result not found.");

        return Ok(new ResultDto(
            r.ResultID,
            r.EnrollmentID,
            r.FinishTime,
            r.Position
        ));
    }
}