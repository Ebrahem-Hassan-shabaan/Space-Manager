using SpaceManager.DTOs.Tickets;
using SpaceManager.Enums;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines service operations for managing workspace maintenance tickets and updates.
/// </summary>
public interface IMaintenanceTicketService
{
    /// <summary>
    /// Reports a new maintenance issue for a workspace.
    /// Validates workspace existence and initializes the ticket in Open status.
    /// </summary>
    /// <param name="userId">The unique identifier of the reporting user.</param>
    /// <param name="dto">The ticket creation payload containing issue details.</param>
    /// <returns>The created maintenance ticket response DTO.</returns>
    Task<TicketResponseDto> CreateMaintenanceTicketAsync(string userId, CreateTicketDto dto);

    /// <summary>
    /// Retrieves a specific maintenance ticket by its ID including its update thread.
    /// Admins can access any ticket; regular members can only retrieve their own.
    /// </summary>
    /// <param name="ticketId">The unique identifier of the ticket.</param>
    /// <param name="userId">Optional user identifier for ownership validation.</param>
    /// <returns>The maintenance ticket response DTO if found; otherwise, null.</returns>
    Task<TicketResponseDto?> GetMaintenanceTicketByIdAsync(int ticketId, string? userId = null);

    /// <summary>
    /// Retrieves maintenance tickets reported by a specific user with optional status and priority filtering.
    /// </summary>
    /// <param name="userId">The unique identifier of the reporting user.</param>
    /// <param name="status">Optional status filter (e.g. Open, InProgress, Resolved, Closed).</param>
    /// <param name="emergencyLevel">Optional emergency level filter (e.g. Low, Medium, High, Critical).</param>
    /// <returns>A collection of matching ticket response DTOs.</returns>
    Task<IEnumerable<TicketResponseDto>> GetUserMaintenanceTicketsAsync(
        string userId,
        TicketStatus? status = null,
        TicketEmergencyLevel? emergencyLevel = null);

    /// <summary>
    /// Retrieves all maintenance tickets across the system with optional filtering. (Admin use only)
    /// </summary>
    /// <param name="status">Optional status filter.</param>
    /// <param name="emergencyLevel">Optional emergency level filter.</param>
    /// <returns>A collection of matching ticket response DTOs.</returns>
    Task<IEnumerable<TicketResponseDto>> GetAllMaintenanceTicketsAsync(
        TicketStatus? status = null,
        TicketEmergencyLevel? emergencyLevel = null);

    /// <summary>
    /// Adds a comment or progress note to an existing ticket thread.
    /// </summary>
    /// <param name="ticketId">The unique identifier of the ticket.</param>
    /// <param name="userId">The unique identifier of the user posting the update.</param>
    /// <param name="isAdmin">Indicates whether the user has administrator privileges.</param>
    /// <param name="dto">The update payload containing the message.</param>
    /// <returns>The created ticket update response DTO.</returns>
    Task<TicketUpdateResponseDto> AddTicketUpdateAsync(
        int ticketId,
        string userId,
        bool isAdmin,
        AddTicketUpdateDto dto);

    /// <summary>
    /// Updates the status of an existing maintenance ticket.
    /// </summary>
    /// <param name="ticketId">The unique identifier of the ticket.</param>
    /// <param name="dto">The new status payload.</param>
    /// <param name="userId">Optional user identifier for ownership check.</param>
    /// <returns>True if the status was updated; otherwise, false.</returns>
    Task<bool> UpdateMaintenanceTicketStatusAsync(int ticketId, UpdateTicketStatusDto dto, string? userId = null);

    /// <summary>
    /// Deletes or revokes a maintenance ticket.
    /// </summary>
    /// <param name="ticketId">The unique identifier of the ticket to delete.</param>
    /// <param name="userId">Optional user identifier for ownership check.</param>
    /// <returns>True if the ticket was deleted; otherwise, false.</returns>
    Task<bool> DeleteMaintenanceTicketAsync(int ticketId, string? userId = null);

    /// <summary>
    /// Deletes an individual ticket update or comment from a ticket's thread.
    /// Restricted to administrator moderation use.
    /// </summary>
    /// <param name="ticketId">The unique identifier of the maintenance ticket.</param>
    /// <param name="updateId">The unique identifier of the ticket update to delete.</param>
    /// <returns>True if the update was found and deleted; otherwise, false.</returns>
    Task<bool> DeleteTicketUpdateAsync(int ticketId, int updateId);
}
