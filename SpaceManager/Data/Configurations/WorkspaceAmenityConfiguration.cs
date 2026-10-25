using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the composite key join entity <see cref="WorkspaceAmenity"/>.
/// </summary>
public class WorkspaceAmenityConfiguration : IEntityTypeConfiguration<WorkspaceAmenity>
{
    /// <summary>
    /// Configures the composite primary key and relationships linking workspaces to amenities.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<WorkspaceAmenity> builder)
    {
        builder.HasKey(wa => new { wa.WorkspaceId, wa.AmenityId });

        builder.HasOne(wa => wa.Workspace)
            .WithMany(w => w.WorkspaceAmenities)
            .HasForeignKey(wa => wa.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wa => wa.Amenity)
            .WithMany(a => a.WorkspaceAmenities)
            .HasForeignKey(wa => wa.AmenityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(wa => wa.IsOperational)
            .IsRequired();
    }
}
