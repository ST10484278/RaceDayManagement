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

    // GET /api/profile/{userId}
    [HttpGet("{userId}")]
    public async Task<ActionResult<ProfileDto>> GetProfile(int userId)
    {
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

    // PUT /api/profile/{userId}
    [HttpPut("{userId}")]
    public async Task<ActionResult<ProfileDto>> UpdateProfile(int userId, [FromBody] UpdateProfileRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
        {
            return NotFound("User profile not found.");
        }

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