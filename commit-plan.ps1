# RaceDay Part 3 - makes one meaningful commit per feature (26 commits).
# Run from the repository root, on your Part 3 branch, AFTER copying the
# new files in:   .\commit-plan.ps1
# Delete this script afterwards (or keep it, it is harmless).

function Save-Commit {
    param([string]$Message, [string[]]$Paths)
    foreach ($p in $Paths) { git add -A -- $p 2>$null }
    git diff --cached --quiet
    if ($LASTEXITCODE -ne 0) {
        git commit -m $Message
    } else {
        Write-Host "Nothing to commit for: $Message"
    }
}

Save-Commit "API: remove unused Azure client wizard code and packages" @("RaceDay.Api/Program.cs","RaceDay.Api/RaceDay.Api.csproj","RaceDay.Api/AzureClientFactoryBuilderExtensions.cs")
Save-Commit "API: blob storage service with private container and image download" @("RaceDay.Api/Services")
Save-Commit "API: event banner upload/download and organiser dashboard endpoint" @("RaceDay.Api/Controllers/EventsController.cs")
Save-Commit "API: profile picture upload and download endpoints" @("RaceDay.Api/Controllers/ProfileController.cs")
Save-Commit "API: organiser can confirm, reset or cancel an enrolment" @("RaceDay.Api/Dtos/UpdateEnrolmentStatusDto.cs","RaceDay.Api/Controllers/EnrolementController.cs")
Save-Commit "API: include total finishers in My Results" @("RaceDay.Api/Controllers/ResultsController.cs")
Save-Commit "API: add blob container name to appsettings" @("RaceDay.Api/appsettings.json")
Save-Commit "Tests: fake blob storage so tests run without Azure" @("RaceDay.Tests/FakeBlobStorageService.cs","RaceDay.Tests/RaceDayWebApplicationFactory.cs")
Save-Commit "Tests: image upload, enrolment status, dashboard and finishers tests" @("RaceDay.Tests/Part3ApiTests.cs","RaceDay.Tests/UnitTest1.cs")
Save-Commit "MVC: remove API settings and packages from the client project" @("RaceDay.Client/RaceDay.Client.csproj","RaceDay.Client/appsettings.json","RaceDay.Client/appsettings.Development.json","RaceDay.Client/Properties/launchSettings.json")
Save-Commit "MVC: configure session, HttpClient, antiforgery and culture" @("RaceDay.Client/Program.cs")
Save-Commit "MVC: ApiService that calls the API with the user's token" @("RaceDay.Client/Services/ApiService.cs","RaceDay.Client/Services/ApiResult.cs")
Save-Commit "MVC: session helpers and role-based SessionAuthorize filter" @("RaceDay.Client/Services/SessionExtensions.cs","RaceDay.Client/Filters")
Save-Commit "MVC: API models, view models and display helpers" @("RaceDay.Client/Models","RaceDay.Client/Helpers")
Save-Commit "MVC: RaceDay theme, role-based layout and shared partials" @("RaceDay.Client/Views/Shared","RaceDay.Client/Views/_ViewImports.cshtml","RaceDay.Client/wwwroot/css","RaceDay.Client/wwwroot/js")
Save-Commit "MVC: home page, registration, login and logout" @("RaceDay.Client/Controllers/BaseController.cs","RaceDay.Client/Controllers/HomeController.cs","RaceDay.Client/Controllers/AccountController.cs","RaceDay.Client/Views/Home","RaceDay.Client/Views/Account")
Save-Commit "MVC: organiser and participant dashboards" @("RaceDay.Client/Controllers/DashboardController.cs","RaceDay.Client/Views/Dashboard")
Save-Commit "MVC: organiser event management with banner upload" @("RaceDay.Client/Controllers/OrganiserEventsController.cs","RaceDay.Client/Views/OrganiserEvents","RaceDay.Client/Controllers/ImagesController.cs")
Save-Commit "MVC: organiser category management" @("RaceDay.Client/Controllers/OrganiserCategoriesController.cs","RaceDay.Client/Views/OrganiserCategories")
Save-Commit "MVC: organiser enrolments page with status colours" @("RaceDay.Client/Controllers/OrganiserEnrolmentsController.cs","RaceDay.Client/Views/OrganiserEnrolments")
Save-Commit "MVC: organiser results entry page" @("RaceDay.Client/Controllers/OrganiserResultsController.cs","RaceDay.Client/Views/OrganiserResults")
Save-Commit "MVC: participant event browsing, details and enrolment" @("RaceDay.Client/Controllers/EventsController.cs","RaceDay.Client/Views/Events")
Save-Commit "MVC: my enrolments and my results pages" @("RaceDay.Client/Controllers/MyEnrolmentsController.cs","RaceDay.Client/Controllers/MyResultsController.cs","RaceDay.Client/Views/MyEnrolments","RaceDay.Client/Views/MyResults")
Save-Commit "MVC: profile page with picture upload" @("RaceDay.Client/Controllers/ProfileController.cs","RaceDay.Client/Views/Profile")
Save-Commit "Docker: Dockerfile, dockerignore and compose file for the MVC app" @("RaceDay.Client/Dockerfile","RaceDay.Client/.dockerignore","docker-compose.yml")
Save-Commit "CI: build API and MVC, run tests and build the Docker image" @(".github")
Save-Commit "Docs: README with architecture, setup and Docker instructions" @("README.md",".gitignore")
Save-Commit "Part 3: final tidy-up" @(".")

git log --oneline | Select-Object -First 30
