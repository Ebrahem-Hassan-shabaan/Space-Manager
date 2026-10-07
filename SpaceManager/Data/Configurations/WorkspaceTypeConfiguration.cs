using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

public class WorkspaceTypeConfiguration : IEntityTypeConfiguration<WorkspaceType>
{
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
