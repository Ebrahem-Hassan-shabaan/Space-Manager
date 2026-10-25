using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the composite key join entity <see cref="BookingAddOn"/>.
/// </summary>
public class BookingAddOnConfiguration : IEntityTypeConfiguration<BookingAddOn>
{
    /// <summary>
    /// Configures the composite primary key and foreign key relationships for booking add-on associations.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<BookingAddOn> builder)
    {
        builder.HasKey(ba => new { ba.WorkspaceBookingId, ba.ServiceAddOnId });

        builder.HasOne(ba => ba.WorkspaceBooking)
            .WithMany(wb => wb.BookingAddOns)
            .HasForeignKey(ba => ba.WorkspaceBookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ba => ba.ServiceAddOn)
            .WithMany(sa => sa.BookingAddOns)
            .HasForeignKey(ba => ba.ServiceAddOnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ba => ba.Quantity)
            .IsRequired();

        builder.Property(ba => ba.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
    }
}
