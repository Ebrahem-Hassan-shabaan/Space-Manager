using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpaceManager.Models;

namespace SpaceManager.Data;

/// <summary>
/// Primary Entity Framework Core database context for the SpaceManager platform,
/// managing identity, workspaces, bookings, billing, and facility operations.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Gets or sets the database set of workspace reservations.
    /// </summary>
    public DbSet<WorkspaceBooking> WorkspaceBookings { get; set; }

    /// <summary>
    /// Gets or sets the database set of physical workspaces.
    /// </summary>
    public DbSet<Workspace> Workspaces { get; set; }

    /// <summary>
    /// Gets or sets the database set of workspace category classifications.
    /// </summary>
    public DbSet<WorkspaceType> WorkspaceTypes { get; set; }

    /// <summary>
    /// Gets or sets the database set of visitor guest passes.
    /// </summary>
    public DbSet<VisitorPass> VisitorPasses { get; set; }

    /// <summary>
    /// Gets or sets the database set of maintenance tickets.
    /// </summary>
    public DbSet<MaintenanceTicket> MaintenanceTickets { get; set; }

    /// <summary>
    /// Gets or sets the database set of ticket update messages and responses.
    /// </summary>
    public DbSet<TicketUpdate> TicketUpdates { get; set; }

    /// <summary>
    /// Gets or sets the database set of workspace amenities.
    /// </summary>
    public DbSet<Amenity> Amenities { get; set; }

    /// <summary>
    /// Gets or sets the database set of workspace-to-amenity assignments.
    /// </summary>
    public DbSet<WorkspaceAmenity> WorkspaceAmenities { get; set; }

    /// <summary>
    /// Gets or sets the database set of service add-ons attached to bookings.
    /// </summary>
    public DbSet<BookingAddOn> BookingAddOns { get; set; }

    /// <summary>
    /// Gets or sets the database set of catalog service add-ons.
    /// </summary>
    public DbSet<ServiceAddOn> ServiceAddOns { get; set; }

    /// <summary>
    /// Gets or sets the database set of corporate companies.
    /// </summary>
    public DbSet<Company> Companies { get; set; }

    /// <summary>
    /// Gets or sets the database set of billing invoices.
    /// </summary>
    public DbSet<Invoice> Invoices { get; set; }

    /// <summary>
    /// Gets or sets the database set of payment transactions.
    /// </summary>
    public DbSet<Payment> Payments { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to configure the database context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the model relationships, entity mappings, and constraints using Fluent API configurations.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
