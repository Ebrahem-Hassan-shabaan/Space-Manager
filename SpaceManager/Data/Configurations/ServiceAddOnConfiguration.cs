using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;  

namespace SpaceManager.Data.Configurations;

public class ServiceAddOnConfiguration : IEntityTypeConfiguration<ServiceAddOn>
{
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
