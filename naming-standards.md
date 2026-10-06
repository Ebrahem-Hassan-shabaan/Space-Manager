# Naming Standards & Conventions

This document establishes the official naming conventions, prefix/suffix guidelines, and formatting standards across the codebase.

---

## 1. General C# Formatting Rules

### File-Scoped Namespaces
Always use modern C# file-scoped namespaces with a semicolon `;` instead of block-scoped namespaces `{ }` to reduce unnecessary indentation and increase readability.

```csharp
// Recommended
namespace SpaceManager.Services;

public class WorkspaceService : IWorkspaceService
{
    // ...
}
```

```csharp
// Avoid
namespace SpaceManager.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        // ...
    }
}
```

---

## 2. Prefixes and Suffixes Reference Table

| Construct | Rule / Convention | Example |
| :--- | :--- | :--- |
| **Interfaces** | Prefix with `I` | `IWorkspaceService`, `IRepository<T>` |
| **Private Fields / Injected Services** | Prefix with `_` + `camelCase` | `private readonly AppDbContext _context;` |
| **Asynchronous Methods** | Suffix with `Async` | `GetWorkspaceByIdAsync(...)` |
| **DTOs (Requests / Responses)** | Suffix with `Request` / `Response` or `Dto` | `CreateWorkspaceRequest`, `WorkspaceResponse` |
| **EF Core Configurations** | Suffix with `Configuration` | `WorkspaceConfiguration`, `VisitorPassConfiguration` |
| **Exceptions** | Suffix with `Exception` | `NotFoundException`, `BookingConflictException` |
| **Controllers** | Suffix with `Controller` | `WorkspacesController`, `BookingsController` |
| **Enums** | **No** `E` prefix, **No** `Enum` suffix. Use descriptive nouns. | `BookingStatus`, `TicketEmergencyLevel` |

---

## 3. Detailed Naming Conventions with Code Snippets

### A. Enums & Enum Members
* **Enum Name:** Singular noun in `PascalCase`. Do **not** use `E...` or `...Enum`.
* **Descriptive Endings:** Use contextual nouns like `Status`, `Level`, `Type`, `Role`.
* **Enum Members:** `PascalCase`. Never prefix values with the enum name.

```csharp
namespace SpaceManager.Enums;

public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
    Completed = 4
}

public enum TicketEmergencyLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
```

---

### B. Classes, Properties, and Variables
* **Classes / Records / Structs:** `PascalCase`
* **Public Properties:** `PascalCase`
* **Method Parameters & Local Variables:** `camelCase`
* **Private Readonly Fields:** `_camelCase`

```csharp
namespace SpaceManager.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<BookingService> _logger;

    public BookingService(AppDbContext context, ILogger<BookingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkspaceBookingResponse> CreateBookingAsync(CreateBookingRequest request, string userId)
    {
        var totalHours = (request.EndTime - request.StartTime).TotalHours;
        
        var booking = new WorkspaceBooking
        {
            UserId = userId,
            WorkspaceId = request.WorkspaceId,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        _context.WorkspaceBookings.Add(booking);
        await _context.SaveChangesAsync();

        return new WorkspaceBookingResponse(booking.Id, booking.Status);
    }
}
```

---

### C. EF Core Configurations
* Place all entity configurations in `Data/Configurations/`.
* File name and class name must end with `Configuration`.
* Implement `IEntityTypeConfiguration<T>`.

```csharp
namespace SpaceManager.Data.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.PricePerHour)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.HasOne(w => w.WorkspaceType)
            .WithMany(wt => wt.Workspaces)
            .HasForeignKey(w => w.WorkspaceTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```
