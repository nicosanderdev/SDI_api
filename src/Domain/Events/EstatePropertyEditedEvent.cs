namespace SDI_Api.Domain.Events;

public class EstatePropertyEditedEvent : BaseEvent
{
    public EstatePropertyEditedEvent(EstateProperty item)
    {
        Item = item;
    }

    public EstateProperty Item { get; }
}
