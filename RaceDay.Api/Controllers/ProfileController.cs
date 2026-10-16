using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ProfileController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET /api/profile/me - Retrieves current session user's profile
    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetMyProfile([FromHeader(Name = "X-User-Id")] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("Invalid session user identity.");
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
        {
            return NotFound("User profile not found.");
        }

        var profile = new ProfileDto(
            UserId: user.UserID.ToString(),
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email,
            Role: user.Role?.RoleName ?? "Participant"
        );

        return Ok(profile);
    }

    // PUT /api/profile/me - Updates current session user's profile only
    [HttpPut("me")]
    public async Task<ActionResult<ProfileDto>> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request,
        [FromHeader(Name = "X-User-Id")] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("Invalid session user identity.");
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
        {
            return NotFound("User profile not found.");
        }

        // Apply profile updates to logged-in user account
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;

        await _context.SaveChangesAsync();

        var updatedProfile = new ProfileDto(
            UserId: user.UserID.ToString(),
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email,
            Role: user.Role?.RoleName ?? "Participant"
        );

        return Ok(updatedProfile);
    }
}