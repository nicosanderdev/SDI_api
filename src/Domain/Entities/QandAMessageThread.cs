namespace SDI_Api.Domain.Entities;

public class QandAMessageThread : BaseAuditableEntity
{
    public Guid EstatePropertyId { get; set; }
    public required EstateProperty EstateProperty { get; set; }
    public required string Title { get; set; }
    public required ICollection<QandAMessage> Messages { get; set; }
}
