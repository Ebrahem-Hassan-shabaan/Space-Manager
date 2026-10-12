using SpaceManager.DTOs.VisitorPasses;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines service operations for managing guest visitor passes.
/// </summary>
public interface IVisitorPassService
{
    /// <summary>
    /// Creates a new visitor pass for a guest linked to an active booking.
    /// Validates booking ownership, ensures the booking is confirmed,
    /// and ensures the expected arrival falls within the reservation time slot.
    /// </summary>
    /// <param name="userId">The unique identifier of the authenticated host user.</param>
    /// <param name="dto">The visitor pass request details.</param>
    /// <returns>The created visitor pass response DTO.</returns>
    Task<VisitorPassResponseDto> CreateVisitorPassAsync(string userId, CreateVisitorPassDto dto);

    /// <summary>
    /// Retrieves a visitor pass by its unique identifier.
    /// If userId is specified, restricts retrieval to passes belonging to that user (ownership check).
    /// If userId is null, returns the pass regardless of host (admin mode).
    /// </summary>
    /// <param name="visitorPassId">The unique identifier of the visitor pass.</param>
    /// <param name="userId">Optional host user identifier for ownership validation.</param>
    /// <returns>The visitor pass response DTO if found; otherwise, null.</returns>
    Task<VisitorPassResponseDto?> GetVisitorPassByIdAsync(int visitorPassId, string? userId = null);

    /// <summary>
    /// Retrieves all visitor passes created by the specified host user.
    /// </summary>
    /// <param name="userId">The unique identifier of the host user.</param>
    /// <returns>A collection of visitor pass response DTOs for the user.</returns>
    Task<IEnumerable<VisitorPassResponseDto>> GetUserVisitorPassesAsync(string userId);

    /// <summary>
    /// Retrieves all visitor passes across the system. (Admin and Reception use only)
    /// </summary>
    /// <returns>A collection of all visitor pass response DTOs.</returns>
    Task<IEnumerable<VisitorPassResponseDto>> GetAllVisitorPassesAsync();

    /// <summary>
    /// Marks a visiting guest as checked in upon arrival.
    /// Records the current timestamp as the actual check-in time.
    /// </summary>
    /// <param name="visitorPassId">The unique identifier of the visitor pass.</param>
    /// <returns>True if the visitor was successfully checked in; false if the pass was not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the guest has already checked in or if the booking is cancelled.</exception>
    Task<bool> CheckInVisitorPassAsync(int visitorPassId);

    /// <summary>
    /// Deletes or revokes a visitor pass.
    /// If userId is specified, ensures the pass belongs to that user.
    /// If userId is null, allows deletion regardless of host (admin mode).
    /// </summary>
    /// <param name="visitorPassId">The unique identifier of the visitor pass to delete.</param>
    /// <param name="userId">Optional host user identifier for ownership validation.</param>
    /// <returns>True if the pass was deleted; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the guest has already checked in.</exception>
    Task<bool> DeleteVisitorPassAsync(int visitorPassId, string? userId = null);
}
