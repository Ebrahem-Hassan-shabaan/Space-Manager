using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="SubscriptionPlan"/> entity.
/// </summary>
public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(sp => sp.Name)
            .IsUnique();

        builder.Property(sp => sp.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(sp => sp.MonthlyPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(sp => sp.DurationMonths)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(sp => sp.MaxBookingsPerMonth)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sp => sp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(sp => sp.WorkspaceType)
            .WithMany()
            .HasForeignKey(sp => sp.WorkspaceTypeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
