namespace Bookify.Application.Interfaces;

/// <summary>
/// Push notification service interface for sending push notifications to mobile and web clients.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Send a push notification to a single device.
    /// </summary>
    Task SendPushNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a push notification to multiple devices.
    /// </summary>
    Task SendPushNotificationToMultipleAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a push notification to a topic (broadcast).
    /// </summary>
    Task SendTopicNotificationAsync(
        string topic,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);
}