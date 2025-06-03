namespace SDI_Api.Domain.Events;

public class QandAMessageCreatedEvent : BaseEvent
{
    public QandAMessageCreatedEvent(QandAMessage item)
    {
        Item = item;
    }

    public QandAMessage Item { get; }
}
