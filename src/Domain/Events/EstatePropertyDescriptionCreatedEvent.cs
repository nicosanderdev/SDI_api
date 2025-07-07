namespace SDI_Api.Domain.Events;

public class EstatePropertyValuesCreatedEvent : BaseEvent
{
    public EstatePropertyValuesCreatedEvent(EstatePropertyValues item)
    {
        Item = item;
    }

    public EstatePropertyValues Item { get; }
}
