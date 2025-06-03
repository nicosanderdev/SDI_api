using Microsoft.Extensions.Logging;
using SDI_Api.Domain.Events;

namespace SDI_Api.Application.EstatePropertyDescriptions.EventHandlers;

public class TodoItemCreatedEventHandler : INotificationHandler<EstatePropertyCreatedEvent>
{
    private readonly ILogger<TodoItemCreatedEventHandler> _logger;

    public TodoItemCreatedEventHandler(ILogger<TodoItemCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(EstatePropertyCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SDI_Api Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}
