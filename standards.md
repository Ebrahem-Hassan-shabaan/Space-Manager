# DeskFlow Architectural & Coding Standards

## 1. Naming Conventions (Strict .NET Standards)

- **Classes, Records, Structs:** `PascalCase` (e.g., `WorkspaceBooking`)
- **Interfaces:** `PascalCase` starting with `I` (e.g., `IWorkspaceService`)
- **Properties & Methods:** `PascalCase` (e.g., `public string FirstName { get; set; }`)
- **Method Parameters & Local Variables:** `camelCase` (e.g., `string userId`)
- **Private Fields (Injected Dependencies):** `_camelCase` (e.g., `private readonly ApplicationDbContext _context;`)
- **Asynchronous Methods:** Must end with the `Async` suffix (e.g., `GetBookingAsync`)

## 2. Architectural Principles (SOLID)

- **Single Responsibility (Fluent API):** Database configurations (foreign keys, string lengths, OnDelete behaviors) must live in `/Data/Configurations` using `IEntityTypeConfiguration<T>`. Never use Data Annotations in the `/Models`.
- **Program to Interfaces:** Controllers must interact with interfaces (`IWorkspaceService`), never concrete classes or the `DbContext` directly.
- **Data Transfer Objects (DTOs):** Database entities are never returned directly to the client. All incoming and outgoing data must be mapped to DTOs in the `/DTOs` folder.
- **Open/Closed Principle (CQRS Lite):** Standard CRUD operations use simple injected services. Complex, high-impact domain actions (e.g., Cancel Reservation) use MediatR to allow future extensibility (emails, refunds) via `INotification` handlers without modifying core logic.

## 3. Background Processing

- **Safe Scoping:** The Automated Status Worker (`IHostedService`) runs as a Singleton. It must inject `IServiceScopeFactory` to open a fresh, temporary scope every 5 minutes to resolve the `ApplicationDbContext`, preventing memory leaks and database locks.
