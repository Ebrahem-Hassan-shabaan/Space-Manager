using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Subscriptions;

/// <summary>
/// Data transfer object used by administrators to update an existing subscription plan.
/// </summary>
public class UpdateSubscriptionPlanDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "Price must be greater than 0 EGP.")]
    public decimal? MonthlyPrice { get; set; }

    [Range(1, 120, ErrorMessage = "Duration must be at least 1 month.")]
    public int? DurationMonths { get; set; }

    [Range(0, 1000, ErrorMessage = "Max bookings must be non-negative (0 for unlimited).")]
    public int? MaxBookingsPerMonth { get; set; }

    public int? WorkspaceTypeId { get; set; }

    public bool? IsActive { get; set; }
}
