using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="UserSubscription"/> entity.
/// </summary>
public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.HasKey(us => us.Id);

        builder.Property(us => us.StartDate)
            .IsRequired();

        builder.Property(us => us.EndDate)
            .IsRequired();

        builder.Property(us => us.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(us => us.IsAutoRenew)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(us => us.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        // Index for fast lookups of a user's active membership
        builder.HasIndex(us => new { us.UserId, us.Status });

        builder.HasOne(us => us.User)
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(us => us.SubscriptionPlan)
            .WithMany(sp => sp.UserSubscriptions)
            .HasForeignKey(us => us.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
