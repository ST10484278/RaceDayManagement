# RaceDayManagement
## 🏃 About RaceDay

**RaceDay** is a full-stack, web-based event management platform built specifically for the South African road running, walking, and cycling community. From iconic events like the Comrades Marathon, Cape Town Cycle Tour, and Soweto Marathon to local weekend park runs, RaceDay modernizes event operations by replacing traditional paper-based registrations and fragmented communications with a centralized, cloud-aware solution.

The platform provides an API-driven ecosystem that allows event organizers to create and manage events, categories, and participant results seamlessly, while giving participants a hub to browse events, register, view real-time route/weather details, and track their personal race performance history.

---

## 👥 User Roles & Responsibilities

RaceDay enforces Role-Based Access Control (RBAC) supporting two core system roles:

### 1. 📋 Event Organiser
Organisers are responsible for managing event operations, logistics, and race performance tracking.

* **Create, Edit, and Delete Events:** Set up race details including titles, descriptions, dates, and locations.
* **Manage Categories:** Define event sub-divisions (e.g., 42.2km Full Marathon, 21.1km Half Marathon, 10km Fun Run) with start locations, distances, and entry fees.
* **View Enrolments:** Access real-time participant registration lists and bib number assignments for managed events.
* **Capture Race Results:** Input and publish participant finish times, overall/category rankings, and completion statuses (`Finished`, `DNF`, `DNS`).

### 2. 🏃 Participant
Participants represent the athletes and runners using the platform to register for events and track their athletic progress.

* **Account Management:** Register a new user account and authenticate securely into the platform.
* **Browse Events & Categories:** Search and explore upcoming road running, walking, and cycling events.
* **Event Entry:** Register for specific event categories and receive unique race bib numbers.
* **Personal Dashboard:** View active event enrolments, prepare using route/weather information, and access historic race performance metrics over time.

## Part 2
Part 2 delivers a fully functional, RESTful Web API backend built with ASP.NET Core and Entity Framework Core. It implements session-like header-based Role-Based Access Control (RBAC), database state management with SQL Server, input validation, comprehensive unit/integration testing with xUnit, OpenAPI documentation via Scalar, and an automated continuous integration pipeline using GitHub Actions.

### Technologies Used
* **Framework:** ASP.NET Core Web API (.NET 9)
* **ORM & Database:** Entity Framework Core 9 with SQL Server
* **Authentication:** Password Hashing (BCrypt.NET) & Session Identity Headers (`X-User-Role`, `X-User-Id`)
* **Testing:** xUnit & `Microsoft.AspNetCore.Mvc.Testing`
* **Documentation:** Scalar OpenAPI UI
* **CI/CD:** GitHub Actions

### Project Structure
```text
RaceDayManagement/
├── .github/
│   └── workflows/
│       └── build-and-test.yml   # GitHub Actions CI/CD Pipeline
├── docs/
│   └── github-actions-green-build.png
├── RaceDay.Api/                 # Core Web API Project (Controllers, Data Context, Models)
├── RaceDay.Contracts/           # Shared DTOs and Data Transfer Objects
└── RaceDay.Api.Tests/           # Automated Integration and Unit Tests (xUnit)