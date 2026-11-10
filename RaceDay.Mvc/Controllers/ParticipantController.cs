using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;

    public ParticipantController(RaceDayApiClient api) => _api = api;

    private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));

    private bool IsParticipant() =>
        string.Equals(HttpContext.Session.GetString("Role"), "Participant", StringComparison.OrdinalIgnoreCase);

    // 7.1 Upcoming events page with filtering
    [HttpGet]
    public async Task<IActionResult> Index(string? type, string? search)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/events");
        var events = await _api.ReadAsync<List<EventDto>>(response) ?? new();

        // Filter upcoming events (Date >= Today)
        events = events.Where(e => e.Date >= DateTime.Today).ToList();

        // Optional filtering by search query across Title and Location
        if (!string.IsNullOrWhiteSpace(search))
        {
            events = events.Where(e =>
                e.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        ViewBag.CurrentSearch = search;
        ViewBag.CurrentType = type;

        return View(events);
    }

    // 7.2 Event details and category selection
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var eventResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        if (!eventResponse.IsSuccessStatusCode) return NotFound();
        var ev = await _api.ReadAsync<EventDto>(eventResponse);

        var catResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}/categories");
        var categories = await _api.ReadAsync<List<CategoryDto>>(catResponse) ?? new();

        var vm = new EventDetailsVm
        {
            Event = ev!,
            Categories = categories
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> EnterEvent(int eventId, int categoryId)
    {
        if (!IsParticipant()) return RedirectToAction("Login", "Auth");

        var request = new CreateEnrolmentRequest(categoryId);

        var response = await _api.SendAsync(HttpMethod.Post, "api/enrolments", request);
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        TempData["Success"] = "You have entered the event successfully.";
        return RedirectToAction(nameof(MyEvents));
    }

    // 7.3 My Events
    [HttpGet]
    public async Task<IActionResult> MyEvents()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/enrolments/mine");
        var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response) ?? new();
        return View(enrolments);
    }

    // 7.4 My Results
    [HttpGet]
    public async Task<IActionResult> MyResults()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/results/mine");
        var results = await _api.ReadAsync<List<ResultDto>>(response) ?? new();
        return View(results);
    }

    // 7.5 Profile & Profile Picture Upload
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");

        var response = await _api.SendAsync(HttpMethod.Get, "api/profile");
        var profile = await _api.ReadAsync<ProfileDto>(response);

        if (profile == null) return NotFound();

        var vm = new ParticipantProfileVm
        {
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = profile.Email
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Profile(ParticipantProfileVm vm, IFormFile? profilePicture)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (!ModelState.IsValid) return View(vm);

        var request = new UpdateProfileRequest(vm.FirstName, vm.LastName, vm.Email);
        var response = await _api.SendAsync(HttpMethod.Put, "api/profile", request);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            return View(vm);
        }

        if (profilePicture != null && profilePicture.Length > 0)
        {
            using var content = new MultipartFormDataContent();
            using var stream = profilePicture.OpenReadStream();
            content.Add(new StreamContent(stream), "file", profilePicture.FileName);

            var uploadResponse = await _api.SendAsync(HttpMethod.Post, "api/profile/upload-image", content);
            if (!uploadResponse.IsSuccessStatusCode)
            {
                TempData["Warning"] = "Profile saved, but image upload failed.";
                return RedirectToAction(nameof(Profile));
            }
        }

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }
}