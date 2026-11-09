# RaceDay

RaceDay is a platform for finding, entering and running races (runs, walks
and cycling events). It is built in three parts:

| Part | What it is | Project |
|------|------------|---------|
| 1-2  | REST API, database (SQL Server) and unit/API tests | `RaceDay.Api`, `RaceDay.Tests` |
| 3    | ASP.NET Core MVC web application, Azure Blob Storage, Docker | `RaceDay.Client` |

**YouTube demo video (unlisted):** `<paste your link here>`

---

## Architecture

```
 Browser  ──►  RaceDay.Client (MVC, session, Bootstrap)
                    │  HTTP + JWT  (HttpClient only)
                    ▼
               RaceDay.Api (REST, JWT, roles)
                 │                    │
                 ▼                    ▼
           SQL Server         Azure Blob Storage
        (events, users,       (event banners and
         enrolments...)        profile pictures)
```

* **The MVC project has no database access, no Azure SDK and no business
  logic.** Every read and write goes to the API through `Services/ApiService.cs`.
* **Images:** the MVC app uploads a file to the API. The API stores it in
  Azure Blob Storage and saves only the *blob name* in SQL. To show an image,
  MVC asks the API, which streams the file from the (private) blob container.
  The MVC application never communicates with Azure directly.
* **Authentication:** the user logs in through the API, which returns a JWT.
  MVC keeps the token, role and name in **session**. The
  `[SessionAuthorize("Organiser")]` filter protects pages by role, and
  every API call forwards the token.

### MVC structure (`RaceDay.Client`)

| Folder | Purpose |
|--------|---------|
| `Controllers/` | One controller per page group (Account, Dashboard, Events, MyEnrolments, MyResults, Profile, Organiser*, Images) |
| `Views/` | Razor views, shared layout, partials (`_Alerts`, `_StatusBadge`, `_EventCard`...) |
| `Models/Api/` | Classes that describe the JSON returned by the API |
| `Models/ViewModels/` | Form and page models with validation attributes |
| `Services/` | `ApiService` (all HTTP calls) and session helpers |
| `Filters/` | `SessionAuthorizeAttribute` (role-based access) |
| `Helpers/` | Formatting (dates, "47th of 312 finishers", status colours) |
| `wwwroot/` | Bootstrap, `site.css` (RaceDay theme), `site.js` |

---

## Features

**Organiser**
* Dashboard: events created, upcoming events, enrolments per event
* Events: create, edit, delete, with a **banner image** upload
* Categories: add, edit and delete age or distance categories per event
* Enrolments: see every participant, their category and status; confirm or cancel
* Results: capture and publish finish time, race number and position after the event

**Participant**
* Home: upcoming events with filters (search, type, distance, date)
* Event page: full details, banner and categories; enrol in a category
* My enrolments: every event entered with its status
* My results: race history, e.g. *47th of 312 finishers*
* Profile: update details and upload a profile picture

**Enrolment status colours:** green = Confirmed, amber = Pending, red = Cancelled.
(The database value `Paid` is displayed as *Confirmed*.)

---

## Running the project locally

### 1. Prerequisites
* .NET 8 SDK
* SQL Server (the connection string is in `RaceDay.Api/appsettings.json`)
* **Azure Blob Storage** *or* the **Azurite** emulator for local development
* Docker Desktop (for the container)

### 2. Configure the API

```powershell
# Database
dotnet ef database update --project RaceDay.Api

# Blob Storage connection string (stored in user-secrets, never in Git)
dotnet user-secrets set "AzureBlob:ConnectionString" "<your connection string>" --project RaceDay.Api
```

Use `UseDevelopmentStorage=true` for Azurite, or the connection string from
**Azure Portal > Storage account > Access keys** for real Azure Blob Storage.
The container (`raceday-images`) is created automatically and stays private.

### 3. Start the API (port 5208)

```powershell
dotnet run --project RaceDay.Api --launch-profile http
```

Swagger: <http://localhost:5208/swagger>

### 4. Start the MVC application (port 5100)

```powershell
dotnet run --project RaceDay.Client --launch-profile http
```

Open <http://localhost:5100>. In Visual Studio you can instead set
**both projects as startup projects** (Solution > Configure Startup Projects).

---

## Docker

The MVC application runs inside a Docker container. The API keeps running on
your computer, and the container reaches it through `host.docker.internal`.

**Step 1 - start the API so the container can reach it** (listen on all interfaces):

```powershell
dotnet run --project RaceDay.Api --urls http://0.0.0.0:5208
```

**Step 2 - build and run the container (single command):**

```powershell
docker compose up --build
```

or, without Compose:

```powershell
docker build -t raceday-mvc ./RaceDay.Client
docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5208/ raceday-mvc
```

Open <http://localhost:8080>.

The `Dockerfile` uses a multi-stage build (SDK image to build and publish,
small ASP.NET runtime image to run), restores packages in a cached layer,
runs as a non-root user and has a `.dockerignore`.

---

## Tests

```powershell
dotnet test
```

The tests use an in-memory database and a fake blob service, so they need
neither SQL Server nor Azure. They cover authentication, role rules, events,
enrolments, results, image upload and download, enrolment confirmation,
the organiser dashboard data and the finishers count.

## CI/CD

`.github/workflows/dotnet.yml` restores, builds (API **and** MVC) and tests the
solution on every push, then builds the MVC Docker image.

## Troubleshooting

| Problem | Fix |
|---------|-----|
| "Address already in use" | Each project needs its own port: API 5208, MVC 5100. Check `Properties/launchSettings.json` of each project. |
| Image upload fails with a connection error | Start Azurite (or check the Azure connection string in user-secrets). |
| Azurite says the API version is not supported | Update Azurite, or start it with `--skipApiVersionCheck`. |
| MVC shows "We could not reach the RaceDay service" | The API is not running or `ApiSettings:BaseUrl` is wrong. |
| Container cannot reach the API | Start the API with `--urls http://0.0.0.0:5208`. |
