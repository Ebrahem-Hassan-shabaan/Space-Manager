using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

public class VisitorPassConfiguration : IEntityTypeConfiguration<VisitorPass>
{
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
