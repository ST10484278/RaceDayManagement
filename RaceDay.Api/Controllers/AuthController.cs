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
        // 1. Validate required fields
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required.");
        }

        // 2. Prevent duplicate accounts
        var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (existingUser)
        {
            return BadRequest("A user with this email already exists.");
        }

        // 3. Resolve or create requested Role entity
        var requestedRoleName = string.IsNullOrWhiteSpace(request.Role) ? "Participant" : request.Role;
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == requestedRoleName);
        if (role == null)
        {
            role = new Role { RoleName = requestedRoleName };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        // 4. Hash password with BCrypt (never save plain text passwords)
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 5. Store user in SQL Server
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

        var response = new AuthResponse(
            Token: "dummy-jwt-token",
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
        // 1. Locate user by email including their Role
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        // 2. Validate BCrypt password hash
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        if (!isPasswordValid)
        {
            return Unauthorized("Invalid email or password.");
        }

        // 3. Extract RoleName (e.g., "Participant" or "Organiser")
        string userRole = user.Role?.RoleName ?? "Participant";

        // 4. Generate response carrying session identity (UserId & Role)
        var response = new AuthResponse(
            Token: $"session-token-user-{user.UserID}", // Token or session identifier
            UserId: user.UserID.ToString(),             // e.g., "5"
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Role: userRole                              // e.g., "Participant"
        );

        return Ok(response);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { Message = "Logout successful" });
    }
}