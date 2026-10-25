using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="Workspace"/> entity.
/// </summary>
public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    /// <summary>
    /// Configures property constraints, decimal pricing precision, and workspace category relations.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.PricePerHour)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.Capacity)
            .IsRequired();

        builder.Property(w => w.IsAvailable)
            .HasDefaultValue(true);

        builder.HasOne(w => w.WorkspaceType)
            .WithMany(wt => wt.Workspaces)
            .HasForeignKey(w => w.WorkspaceTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
