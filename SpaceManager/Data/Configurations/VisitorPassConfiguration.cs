using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="VisitorPass"/> entity.
/// </summary>
public class VisitorPassConfiguration : IEntityTypeConfiguration<VisitorPass>
{
    /// <summary>
    /// Configures property constraints, string lengths, and host/booking foreign keys for visitor passes.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<VisitorPass> builder)
    {
        builder.HasKey(vp => vp.Id);

        builder.Property(vp => vp.GuestName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(vp => vp.GuestEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(vp => vp.ExpectedArrival)
            .IsRequired();

        builder.Property(vp => vp.ActualCheckInTime)
            .IsRequired(false);

        builder.HasOne(vp => vp.WorkspaceBooking)
            .WithMany(wb => wb.VisitorPasses)
            .HasForeignKey(vp => vp.WorkspaceBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vp => vp.User)
            .WithMany(u => u.VisitorPasses)
            .HasForeignKey(vp => vp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
