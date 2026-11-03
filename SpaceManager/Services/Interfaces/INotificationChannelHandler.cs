using System.Threading;
using System.Threading.Tasks;
using SpaceManager.Common;
using SpaceManager.DTOs.Notifications;
using SpaceManager.Enums;

namespace SpaceManager.Services.Interfaces;

/// <summary>
/// Defines the contract for concrete notification channel dispatchers, adhering to the Open/Closed Principle
/// via the Self-Selecting Strategy Pattern (analogous to <see cref="IPaymentGateway"/>).
/// </summary>
public interface INotificationChannelHandler
{
    /// <summary>
    /// Evaluates whether this handler supports delivery over the specified channel.
    /// </summary>
    /// <param name="channel">The candidate notification channel.</param>
    /// <returns>True if this handler can process the message; otherwise, false.</returns>
    bool CanHandle(NotificationChannel channel);

    /// <summary>
    /// Delivers the notification message through this channel.
    /// </summary>
    /// <param name="message">The notification details and recipient payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A service result indicating dispatch outcome.</returns>
    Task<ServiceResult> SendAsync(NotificationMessage message, CancellationToken ct = default);
}
