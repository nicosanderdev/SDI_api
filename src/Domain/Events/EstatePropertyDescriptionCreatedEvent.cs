namespace SDI_Api.Domain.Events;

public class EstatePropertyDescriptionCreatedEvent : BaseEvent
{
    public EstatePropertyDescriptionCreatedEvent(EstatePropertyDescription item)
    {
        Item = item;
    }

    public EstatePropertyDescription Item { get; }
}
