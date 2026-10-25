using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

/// <summary>
/// Entity Framework Core mapping configuration for the <see cref="TicketUpdate"/> entity.
/// </summary>
public class TicketUpdateConfiguration : IEntityTypeConfiguration<TicketUpdate>
{
    /// <summary>
    /// Configures message text limits, defaults, and cascade behaviors for ticket updates.
    /// </summary>
    /// <param name="builder">The entity type builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<TicketUpdate> builder)
    {
        builder.HasKey(tu => tu.Id);

        builder.Property(tu => tu.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(tu => tu.Timestamp)
            .IsRequired();

        builder.Property(tu => tu.IsAdminReply)
            .HasDefaultValue(false);

        builder.HasOne(tu => tu.MaintenanceTicket)
            .WithMany(mt => mt.TicketUpdates)
            .HasForeignKey(tu => tu.MaintenanceTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tu => tu.User)
            .WithMany(u => u.TicketUpdates)
            .HasForeignKey(tu => tu.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
