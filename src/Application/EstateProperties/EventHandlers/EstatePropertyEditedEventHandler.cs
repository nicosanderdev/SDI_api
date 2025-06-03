using Microsoft.Extensions.Logging;
using SDI_Api.Domain.Events;

namespace SDI_Api.Application.EstateProperties.EventHandlers;

public class EstatePropertyEditedEventHandler : INotificationHandler<EstatePropertyEditedEvent>
{
    private readonly ILogger<EstatePropertyEditedEventHandler> _logger;

    public EstatePropertyEditedEventHandler(ILogger<EstatePropertyEditedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(EstatePropertyEditedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SDI_Api Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}
