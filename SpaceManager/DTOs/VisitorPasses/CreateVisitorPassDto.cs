using System.ComponentModel.DataAnnotations;

namespace SpaceManager.DTOs.VisitorPasses;

/// <summary>
/// Data transfer object for requesting a new visitor pass.
/// </summary>
/// <param name="WorkspaceBookingId">The unique identifier of the confirmed booking associated with the visit.</param>
/// <param name="GuestName">The full name of the visiting guest.</param>
/// <param name="GuestEmail">The email address of the visiting guest.</param>
/// <param name="ExpectedArrival">The expected arrival date and time (must fall within the booking time slot).</param>
public record CreateVisitorPassDto(
    [Required]
    int WorkspaceBookingId,

    [Required]
    [MaxLength(100)]
    string GuestName,

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    string GuestEmail,

    [Required]
    DateTime ExpectedArrival
);
