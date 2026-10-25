using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="WorkspaceType"/> entity.
/// </summary>
public class WorkspaceTypeConfiguration : IEntityTypeConfiguration<WorkspaceType>
{
    /// <summary>
    /// Configures unique index constraints and string lengths for workspace type classifications.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<WorkspaceType> builder)
    {
        builder.HasKey(wt => wt.Id);

        builder.Property(wt => wt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(wt => wt.Name)
            .IsUnique();
    }
}
