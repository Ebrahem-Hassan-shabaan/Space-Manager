using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

public class WorkspaceAmenityConfiguration : IEntityTypeConfiguration<WorkspaceAmenity>
{
    public void Configure(EntityTypeBuilder<WorkspaceAmenity> builder)
    {
        builder.HasKey(wa => new { wa.WorkspaceId, wa.AmenityId });
        builder.HasOne(wa => wa.Workspace)
            .WithMany(w => w.WorkspaceAmenities)
            .HasForeignKey(wa => wa.WorkspaceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(wa => wa.Amenity).WithMany(a => a.WorkspaceAmenities)
            .HasForeignKey(wa => wa.AmenityId).OnDelete(DeleteBehavior.Restrict); 

        builder.Property(wa => wa.IsOperational)
            .IsRequired();
    }
}

