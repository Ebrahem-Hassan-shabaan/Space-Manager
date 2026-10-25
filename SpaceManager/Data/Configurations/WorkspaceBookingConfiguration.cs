using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="WorkspaceBooking"/> entity.
/// </summary>
public class WorkspaceBookingConfiguration : IEntityTypeConfiguration<WorkspaceBooking>
{
    /// <summary>
    /// Configures decimal precision, required schedule timestamps, and foreign key rules for workspace reservations.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<WorkspaceBooking> builder)
    {
        builder.HasKey(wb => wb.Id);

        builder.Property(wb => wb.StartTime)
            .IsRequired();

        builder.Property(wb => wb.EndTime)
            .IsRequired();

        builder.Property(wb => wb.TotalCost)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.HasOne(wb => wb.Workspace)
            .WithMany(w => w.WorkspaceBookings)
            .HasForeignKey(wb => wb.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wb => wb.User)
            .WithMany(u => u.WorkspaceBookings)
            .HasForeignKey(wb => wb.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
