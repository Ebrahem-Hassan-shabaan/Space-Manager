using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="WorkspaceReview"/> entity.
/// </summary>
public class WorkspaceReviewConfiguration : IEntityTypeConfiguration<WorkspaceReview>
{
    public void Configure(EntityTypeBuilder<WorkspaceReview> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // Enforce 1:1 relation between a completed booking and its review
        builder.HasIndex(r => r.WorkspaceBookingId)
            .IsUnique();

        builder.HasOne(r => r.WorkspaceBooking)
            .WithOne()
            .HasForeignKey<WorkspaceReview>(r => r.WorkspaceBookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Workspace)
            .WithMany()
            .HasForeignKey(r => r.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
