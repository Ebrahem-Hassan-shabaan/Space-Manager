using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SpaceManager.Models;

namespace SpaceManager.Data.Configurations;

public class MaintenanceTicketConfiguration : IEntityTypeConfiguration<MaintenanceTicket>
{
    public void Configure(EntityTypeBuilder<MaintenanceTicket> builder)
    {
        builder.HasKey(mt => mt.Id);

        builder.Property(mt => mt.IssueTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(mt => mt.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(mt => mt.EmergencyLevel)
            .IsRequired();

        builder.Property(mt => mt.Status)
            .IsRequired();

        builder.Property(mt => mt.ReportedAt)
            .IsRequired();

        builder.HasOne(mt => mt.Workspace)
            .WithMany(w => w.MaintenanceTickets)
            .HasForeignKey(mt => mt.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mt => mt.User)
            .WithMany(u => u.MaintenanceTickets)
            .HasForeignKey(mt => mt.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
