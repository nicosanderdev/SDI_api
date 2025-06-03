namespace SDI_Api.Domain.Events;

public class EstatePropertyCreatedEvent : BaseEvent
{
    public EstatePropertyCreatedEvent(EstateProperty item)
    {
        Item = item;
    }

    public EstateProperty Item { get; }
}
