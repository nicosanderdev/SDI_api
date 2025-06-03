using Microsoft.Extensions.Logging;
using SDI_Api.Domain.Events;

namespace SDI_Api.Application.EstateProperties.EventHandlers;

public class EstatePropertyCreatedEventHandler : INotificationHandler<EstatePropertyCreatedEvent>
{
    private readonly ILogger<EstatePropertyCreatedEventHandler> _logger;

    public EstatePropertyCreatedEventHandler(ILogger<EstatePropertyCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(EstatePropertyCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SDI_Api Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}
