# Project Structure & Architecture Guide

This document defines the folder structure and organization rules for the SpaceManager project.

---

## 1. Directory Tree Overview

```text
SpaceManager/
├── Controllers/              # API Endpoints & Request Routing
├── Data/
│   ├── AppDbContext.cs       # EF Core Database Context
│   └── Configurations/       # Fluent API entity configurations (IEntityTypeConfiguration<T>)
├── DTOs/                     # Data Transfer Objects (Requests & Responses)
│   ├── Bookings/
│   ├── Tickets/
│   └── Workspaces/
├── Enums/                    # Strongly-typed enumerations
├── Interfaces/               # Service & Repository interfaces (I...Service)
├── Models/                   # POCO Domain Entities (database entities)
├── Services/                 # Business logic implementation
├── appsettings.json          # Production configuration & logging
├── appsettings.Development.json # Local connection strings & settings
└── Program.cs                # App bootstrap, DI registrations, Middleware pipeline
```

---

## 2. Directory Responsibilities

### `Models/`
Contains pure POCO domain entities representing database tables.
* Use file-scoped namespaces (`namespace SpaceManager.Models;`).
* Keep navigation properties and explicit Foreign Key IDs together.
* Do not clutter entities with Fluent API configuration attributes; keep them clean.

```csharp
namespace SpaceManager.Models;

public class Workspace
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerHour { get; set; }
    public int Capacity { get; set; }
    public bool IsAvailable { get; set; } = true;

    public int WorkspaceTypeId { get; set; }
    public WorkspaceType WorkspaceType { get; set; } = null!;

    public ICollection<WorkspaceBooking> WorkspaceBookings { get; set; } = new List<WorkspaceBooking>();
}
```

---

### `Data/Configurations/`
Houses all database mapping logic using Entity Framework Core's Fluent API.
* Every entity configuration implements `IEntityTypeConfiguration<T>`.
* Loaded automatically via `modelBuilder.ApplyConfigurationsFromAssembly(...)` in `AppDbContext`.

```csharp
namespace SpaceManager.Data.Configurations;

public class WorkspaceBookingConfiguration : IEntityTypeConfiguration<WorkspaceBooking>
{
    public void Configure(EntityTypeBuilder<WorkspaceBooking> builder)
    {
        builder.HasKey(wb => wb.Id);
        
        builder.Property(wb => wb.TotalCost)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.HasOne(wb => wb.Workspace)
            .WithMany(w => w.WorkspaceBookings)
            .HasForeignKey(wb => wb.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

### `DTOs/`
Data Transfer Objects used to receive client requests and return API responses.
* Grouped into subfolders by domain feature (`Workspaces/`, `Bookings/`, etc.).
* Database models must never be returned directly from controllers.

```csharp
namespace SpaceManager.DTOs.Workspaces;

public record CreateWorkspaceRequest(
    string Name,
    decimal PricePerHour,
    int Capacity,
    int WorkspaceTypeId
);

public record WorkspaceResponse(
    int Id,
    string Name,
    decimal PricePerHour,
    int Capacity,
    bool IsAvailable,
    string WorkspaceTypeName
);
```

---

### `Interfaces/` & `Services/`
Encapsulates domain logic away from API controllers.
* Interfaces live in `Interfaces/` (e.g., `IWorkspaceService.cs`).
* Implementations live in `Services/` (e.g., `WorkspaceService.cs`).

```csharp
namespace SpaceManager.Interfaces;

public interface IWorkspaceService
{
    Task<IEnumerable<WorkspaceResponse>> GetAvailableWorkspacesAsync();
    Task<WorkspaceResponse?> GetWorkspaceByIdAsync(int id);
    Task<WorkspaceResponse> CreateWorkspaceAsync(CreateWorkspaceRequest request);
}
```

---

### `Controllers/`
Lightweight controllers responsible only for HTTP status codes and model validation.
* Controllers inject interfaces from `Interfaces/`.
* No database queries or raw SQL directly inside controllers.

```csharp
namespace SpaceManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAvailable()
    {
        var workspaces = await _workspaceService.GetAvailableWorkspacesAsync();
        return Ok(workspaces);
    }
}
```
