using SpaceManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SpaceManager.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasMany(c => c.Users)
               .WithOne(u => u.Company)
               .HasForeignKey(u => u.CompanyId)
               .OnDelete(DeleteBehavior.SetNull);
        builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(c => c.TaxRegistrationNumber)
                .IsRequired()
                .HasMaxLength(50);
        builder.Property(c => c.BillingAddress)
                .IsRequired()
                 .HasMaxLength(200);
        builder .Property(c => c.ContactEmail)
                .IsRequired()
                .HasMaxLength(100);

    }
}
