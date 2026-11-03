using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.Subscriptions;

/// <summary>
/// Data transfer object used by administrators to create a new membership subscription plan.
/// </summary>
public class CreateSubscriptionPlanDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 1_000_000, ErrorMessage = "Price must be greater than 0 EGP.")]
    public decimal MonthlyPrice { get; set; }

    [Required]
    [Range(1, 120, ErrorMessage = "Duration must be at least 1 month.")]
    public int DurationMonths { get; set; } = 1;

    [Range(0, 1000, ErrorMessage = "Max bookings must be non-negative (0 for unlimited).")]
    public int MaxBookingsPerMonth { get; set; } = 0;

    /// <summary>
    /// Optional workspace type classification ID. Null indicates an All-Access Pass.
    /// </summary>
    public int? WorkspaceTypeId { get; set; }
}
