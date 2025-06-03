using Microsoft.Extensions.Logging;
using SDI_Api.Domain.Events;

namespace SDI_Api.Application.EstateProperties.EventHandlers;

public class QandAMessageCreatedEventHandler : INotificationHandler<QandAMessageCreatedEvent>
{
    private readonly ILogger<QandAMessageCreatedEventHandler> _logger;

    public QandAMessageCreatedEventHandler(ILogger<QandAMessageCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(QandAMessageCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SDI_Api Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}
