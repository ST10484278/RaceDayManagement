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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _api.SendAsync(HttpMethod.Post, "api/auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Account registered successfully! Please log in.";
                return RedirectToAction("Login");
            }

            // Convert non-success API response into friendly ModelState error
            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, !string.IsNullOrWhiteSpace(errorContent)
                ? errorContent
                : "Registration failed. Please verify your information and try again.");
        }
        catch (HttpRequestException)
        {
            // Intercept connection failures cleanly without leaking raw trace
            TempData["ErrorMessage"] = "Unable to reach the RaceDay server. Please ensure the backend service is active.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An unexpected error occurred while processing your request. Please try again later.";
        }

        return View(model);
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

        // 2. Store user session info
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