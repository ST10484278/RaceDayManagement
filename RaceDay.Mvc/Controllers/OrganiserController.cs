using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers;

public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;

    public OrganiserController(RaceDayApiClient api) => _api = api;

    private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));

    private bool IsOrganiser() =>
        string.Equals(HttpContext.Session.GetString("Role"), "Organiser", StringComparison.OrdinalIgnoreCase);

    // 6.1 Dashboard
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (!IsOrganiser()) return RedirectToAction("Index", "Home");

        var response = await _api.SendAsync(HttpMethod.Get, "api/events");
        var events = await _api.ReadAsync<List<EventDto>>(response) ?? new();

        var dashboardVm = new OrganiserDashboardVm
        {
            Events = events.Select(e => new OrganiserEventSummaryVm
            {
                EventId = e.Id,
                Name = e.Title,
                EventDate = e.Date,
                EnrolmentCount = 0
            }).ToList()
        };

        return View(dashboardVm);
    }

    // 6.2 Event Management
    [HttpGet]
    public async Task<IActionResult> Events()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (!IsOrganiser()) return RedirectToAction("Index", "Home");

        var response = await _api.SendAsync(HttpMethod.Get, "api/events");
        var events = await _api.ReadAsync<List<EventDto>>(response) ?? new();
        return View(events);
    }

    [HttpGet]
    public IActionResult CreateEvent()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (!IsOrganiser()) return RedirectToAction("Index", "Home");

        return View(new EventFormVm());
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(EventFormVm vm)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (!IsOrganiser()) return RedirectToAction("Index", "Home");
        if (!ModelState.IsValid) return View(vm);

        var request = new CreateEventRequest(
            vm.Title,
            vm.Description,
            vm.Date,
            vm.Location
        );

        var response = await _api.SendAsync(HttpMethod.Post, "api/events", request);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            return View(vm);
        }

        TempData["Success"] = "Event created successfully!";
        return RedirectToAction(nameof(Events));
    }

    // 6.4 Categories
    [HttpGet]
    public async Task<IActionResult> Categories(int eventId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (!IsOrganiser()) return RedirectToAction("Index", "Home");

        var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{eventId}/categories");
        var categories = await _api.ReadAsync<List<CategoryDto>>(response) ?? new();
        ViewBag.EventId = eventId;
        return View(categories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(int eventId, string name, decimal distanceKm)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (!IsOrganiser()) return RedirectToAction("Index", "Home");

        var request = new CreateCategoryRequest(eventId, name, distanceKm);
        await _api.SendAsync(HttpMethod.Post, "api/categories", request);
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    // 6.5 Enrolments
    [HttpGet]
    public async Task<IActionResult> Enrolments(int? eventId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (!IsOrganiser()) return RedirectToAction("Index", "Home");

        var response = await _api.SendAsync(HttpMethod.Get, "api/enrolments");
        var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response) ?? new();

        if (eventId.HasValue)
        {
            enrolments = enrolments.Where(e => e.CategoryId == eventId.Value).ToList();
        }

        return View(enrolments);
    }

    // 6.6 Results Entry
    [HttpGet]
    public IActionResult CaptureResult(int enrolmentId)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (!IsOrganiser()) return RedirectToAction("Index", "Home");

        return View(new CaptureResultVm { EnrolmentId = enrolmentId });
    }

    [HttpPost]
    public async Task<IActionResult> CaptureResult(CaptureResultVm vm)
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
        if (!IsOrganiser()) return RedirectToAction("Index", "Home");
        if (!ModelState.IsValid) return View(vm);

        var request = new CreateResultRequest(vm.EnrolmentId, vm.FinishTime, vm.FinishingPosition);
        var response = await _api.SendAsync(HttpMethod.Post, "api/results", request);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            return View(vm);
        }

        TempData["Success"] = "Result captured successfully!";
        return RedirectToAction(nameof(Enrolments));
    }
}