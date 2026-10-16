using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public AuthController(RaceDayDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        // 1. Receive personal details and basic validation
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required.");
        }

        // 2. Check whether the email address already exists
        var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (existingUser)
        {
            return BadRequest("A user with this email address already exists.");
        }

        // 3. Confirm the selected role is valid / resolve entity
        var requestedRoleName = string.IsNullOrWhiteSpace(request.Role) ? "Participant" : request.Role;
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == requestedRoleName);
        if (role == null)
        {
            role = new Role { RoleName = requestedRoleName };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        // 4. Hash the password using BCrypt (never store plain text)
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 5. Save the user to SQL Server
        var user = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = hashedPassword,
            RoleID = role.RoleID
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 6. Return appropriate success response with session payload
        var response = new AuthResponse(
            Token: $"session-token-user-{user.UserID}",
            UserId: user.UserID.ToString(),
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Role: role.RoleName
        );

        return CreatedAtAction(nameof(Register), new { id = user.UserID }, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        // 1. Find the user using the supplied email
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        // 2. Verify the supplied password against the stored hash
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        if (!isPasswordValid)
        {
            return Unauthorized("Invalid email or password.");
        }

        // 3. Create session with stored UserId and Role identity
        string userRole = user.Role?.RoleName ?? "Participant";

        // 4. Return useful authenticated-user information
        var response = new AuthResponse(
            Token: $"session-token-user-{user.UserID}",
            UserId: user.UserID.ToString(),
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Role: userRole
        );

        return Ok(response);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear session on client-side state
        return Ok(new { Message = "Logout successful. Session cleared." });
    }
}