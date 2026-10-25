using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="ServiceAddOn"/> entity.
/// </summary>
public class ServiceAddOnConfiguration : IEntityTypeConfiguration<ServiceAddOn>
{
    /// <summary>
    /// Configures property constraints, string limits, and decimal types for catalog service add-ons.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<ServiceAddOn> builder)
    {
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sa => sa.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(sa => sa.IsAvailable)
            .IsRequired();
    }
}
