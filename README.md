# MyApplication.Web: A Deep Dive into a Modern .NET 6 MVC Solution

## Table of Contents
- [Project Philosophy](#project-philosophy)
- [Solution Architecture](#solution-architecture)
- [Controllers: The Heart of Business Logic](#controllers-the-heart-of-business-logic)
- [Models: Data Structures and Domain](#models-data-structures-and-domain)
- [Views: User Experience and UI Flow](#views-user-experience-and-ui-flow)
- [Services: Extending Core Capabilities](#services-extending-core-capabilities)
- [Database & Configuration](#database--configuration)
- [Session, Authentication, and Security](#session-authentication-and-security)
- [Custom Middleware](#custom-middleware)
- [Docker & Deployment](#docker--deployment)
- [Developer Experience & Extensibility](#developer-experience--extensibility)
- [Troubleshooting & FAQ](#troubleshooting--faq)
- [Credits & Further Reading](#credits--further-reading)

---

## Project Philosophy

**MyApplication.Web** is not just another MVC project. It is designed as a robust, extensible, and secure job tracking platform, built with modern .NET 6 paradigms. The project emphasizes:
- **Separation of concerns**: Clear boundaries between data, business logic, and presentation.
- **Security by default**: Cookie authentication, session management, and anti-caching headers.
- **Developer empowerment**: Easy to extend, debug, and deploy.
- **Containerization**: Ready for cloud-native environments with Docker and Compose.

## Solution Architecture

- **Framework**: ASP.NET Core MVC (net6.0)
- **Database**: PostgreSQL (via Entity Framework Core)
- **Authentication**: Cookie-based, with session support
- **Frontend**: Razor Views, Bootstrap, jQuery
- **Containerization**: Docker, Docker Compose
- **Configuration**: Environment-based via `appsettings.json` and environment variables

### Directory Structure
```
MyApplication.Web/
├── Controllers/         # MVC Controllers (Home, MainPage, BusinessPage, ForEditProfile)
├── Models/              # Data models (User, Task, etc.)
├── Views/               # Razor views, organized by controller
├── Services/            # Custom services (e.g., EmailValidatorService)
├── Data/                # EF Core DbContext and migrations
├── wwwroot/             # Static files (css, js, images)
├── appsettings.json     # Main configuration
├── Dockerfile           # Container build instructions
├── docker-compose.yml   # Multi-container orchestration
└── Program.cs           # Application entry point
```

## Controllers: The Heart of Business Logic

- **HomeController**: Handles authentication, registration, profile, and task list views.
- **MainPageController**: Manages the main dashboard and user-specific landing pages.
- **BusinessPageController**: CRUD operations for business tasks, including task creation and description editing.
- **ForEditProfileController**: User profile editing and validation.

Each controller is tightly coupled with its respective views and models, ensuring a clean flow from HTTP request to rendered HTML.

## Models: Data Structures and Domain

- **User**: Represents application users, with properties for authentication and profile data.
- **Task**: Core entity for job tracking, including title, description, status, timestamps, and user association.
- **LoginModel, EditProfileModel, ErrorViewModel, BusinessPageViewModel**: View models for specific UI scenarios, ensuring strong typing and validation.

All models are designed for extensibility, with Entity Framework Core attributes and relationships.

## Views: User Experience and UI Flow

Views are organized by controller, following the convention:
- `Views/Home/Index.cshtml`: Landing page
- `Views/Home/Register.cshtml`: User registration
- `Views/Home/Profile.cshtml`: User profile
- `Views/Home/TaskList.cshtml`: Task management
- `Views/MainPage/Index.cshtml`, `Views/BusinessPage/Index.cshtml`, etc.

Shared layouts and partials ensure a consistent look and feel, leveraging Bootstrap and jQuery for responsive, interactive UI.

## Services: Extending Core Capabilities

- **EmailValidatorService**: Injected via DI, this service encapsulates email validation logic, demonstrating how to add custom business services.
- Additional services can be registered in `Program.cs` for further extensibility.

## Database & Configuration

- **Database**: PostgreSQL, configured via `appsettings.json` and environment variables. Connection string example:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=JobTrackingDB;Username=yigit;Password=123"
  }
  ```
- **Migrations**: Managed via EF Core. To add a migration:
  ```sh
  dotnet ef migrations add MigrationName
  dotnet ef database update
  ```
- **Configuration**: Supports multiple environments (Development, Production) via `appsettings.Development.json` and environment variables.

## Session, Authentication, and Security

- **Authentication**: Cookie-based, with secure, HTTP-only cookies and sliding expiration.
- **Session**: 30-minute idle timeout, essential cookies only.
- **Security**: Enforced HTTPS, HSTS in production, anti-caching headers for sensitive routes, and secure cookie policies.

## Custom Middleware

A custom middleware is registered to set cache-control headers, ensuring that sensitive pages are never cached by browsers or proxies:
```csharp
app.Use(async (context, next) =>
{
    if (!context.Request.Path.Value.StartsWith("/css") &&
        !context.Request.Path.Value.StartsWith("/js") &&
        !context.Request.Path.Value.StartsWith("/lib") &&
        !context.Request.Path.Value.StartsWith("/images"))
    {
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, private";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
    }
    await next();
});
```

## Docker & Deployment

### Building the Application
```sh
docker build -t myapp:latest .
```

### Running with Docker Compose
```sh
docker-compose up --build
```
This will start both the web application and a PostgreSQL database, with data persisted in a Docker volume.

- **Web**: Exposed on port 5000 (mapped to container port 80)
- **Postgres**: Exposed on port 6667 (mapped to container port 5432)

### Environment Variables
Sensitive configuration (like connection strings) can be overridden via environment variables in `docker-compose.yml`.

## Developer Experience & Extensibility

- **Hot reload**: Supported via `dotnet watch run` for rapid development.
- **Dependency Injection**: Add new services in `Program.cs` and inject them into controllers or views.
- **Modular Views**: Add new Razor views under the appropriate controller directory.
- **Database Evolution**: Use EF Core migrations for schema changes.
- **Testing**: Easily mock services and controllers for unit/integration testing.

## Troubleshooting & FAQ

- **Database connection issues**: Ensure Docker is running and the `postgres` service is healthy. Check credentials in `appsettings.json` and `docker-compose.yml`.
- **Port conflicts**: Change the `ports` mapping in `docker-compose.yml` if 5000 or 6667 are in use.
- **Session/authentication problems**: Clear browser cookies and restart the application.
- **Adding new features**: Follow the existing pattern—create a model, controller, and views, then wire up routes in `Program.cs`.

## Credits & Further Reading

- Built with [ASP.NET Core MVC](https://docs.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-6.0)
- Database powered by [PostgreSQL](https://www.postgresql.org/)
- Containerized with [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/)
- UI styled with [Bootstrap](https://getbootstrap.com/) and [jQuery](https://jquery.com/)

---

> This README was crafted to provide a deep, unique, and practical understanding of the project, going beyond boilerplate documentation. For questions, suggestions, or contributions, please open an issue or pull request. 