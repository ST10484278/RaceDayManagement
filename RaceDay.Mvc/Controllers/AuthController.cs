using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public class AuthController : Controller
{
    private readonly RaceDayApiClient _api;

    public AuthController(RaceDayApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Matches: RegisterRequest(string FirstName, string LastName, string Email, string Password, string Role)
        var request = new RegisterRequest(
            vm.FirstName,
            vm.LastName,
            vm.Email,
            vm.Password,
            vm.Role
        );

        var response = await _api.SendAsync(HttpMethod.Post, "api/auth/register", request);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            return View(vm);
        }

        TempData["Success"] = "Registration successful. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Matches: LoginRequest(string Email, string Password)
        var request = new LoginRequest(vm.Email, vm.Password);

        var response = await _api.SendAsync(HttpMethod.Post, "api/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(vm);
        }

        var auth = await _api.ReadAsync<AuthResponse>(response);
        if (auth == null)
        {
            ModelState.AddModelError("", "Login response could not be read.");
            return View(vm);
        }

        // 1. Preserve API session cookie
        string? apiCookie = RaceDayApiClient.ExtractSessionCookie(response);
        if (!string.IsNullOrWhiteSpace(apiCookie))
            HttpContext.Session.SetString("ApiSessionCookie", apiCookie);

        // 2. Store user session info (auth.UserId is string so we use SetString)
        HttpContext.Session.SetString("UserId", auth.UserId);
        HttpContext.Session.SetString("Role", auth.Role);
        HttpContext.Session.SetString("DisplayName", auth.FirstName ?? auth.Email);

        if (auth.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction("Index", "Organiser");

        return RedirectToAction("Index", "Participant");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _api.SendAsync(HttpMethod.Post, "api/auth/logout");
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}