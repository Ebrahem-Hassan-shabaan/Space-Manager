using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpaceManager.Models;

namespace SpaceManager.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<WorkspaceBooking> WorkspaceBookings { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<WorkspaceType> WorkspaceTypes { get; set; }
    public DbSet<VisitorPass> VisitorPasses { get; set; }
    public DbSet<MaintenanceTicket> MaintenanceTickets { get; set; }
    public DbSet<TicketUpdate> TicketUpdates { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
