namespace SDI_Api.Domain.Events;

public class QandAMessageCreatedEvent : BaseEvent
{
    public QandAMessageCreatedEvent(Message item)
    {
        Item = item;
    }

    public Message Item { get; }
}
