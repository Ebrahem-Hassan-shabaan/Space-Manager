using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

public class TicketUpdateConfiguration : IEntityTypeConfiguration<TicketUpdate>
{
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
