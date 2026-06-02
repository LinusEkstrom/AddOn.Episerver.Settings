using System.Threading;
using System.Threading.Tasks;
using EPiServer.Applications;
using EPiServer.Events;

namespace AddOn.Episerver.Settings.Core;

/// <summary>
///     Updates settings roots when applications are created or updated.
/// </summary>
public class SettingsApplicationChangedSubscriber :
    IEventSubscriber<ApplicationCreatedEvent>,
    IEventSubscriber<ApplicationUpdatedEvent>
{
    private readonly ISettingsService settingsService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsApplicationChangedSubscriber" /> class.
    /// </summary>
    /// <param name="settingsService">The settings service.</param>
    public SettingsApplicationChangedSubscriber(ISettingsService settingsService)
    {
        this.settingsService = settingsService;
    }

    /// <summary>
    ///     Handles application creation events.
    /// </summary>
    /// <param name="eventData">The application creation event.</param>
    /// <param name="context">The event context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task HandleAsync(
        ApplicationCreatedEvent eventData,
        EventContext context,
        CancellationToken cancellationToken = default)
    {
        settingsService.ValidateOrCreateSiteSettingsRoot(eventData.Application);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles application update events.
    /// </summary>
    /// <param name="eventData">The application update event.</param>
    /// <param name="context">The event context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task HandleAsync(
        ApplicationUpdatedEvent eventData,
        EventContext context,
        CancellationToken cancellationToken = default)
    {
        settingsService.ValidateOrCreateSiteSettingsRoot(eventData.Application);
        return Task.CompletedTask;
    }
}
