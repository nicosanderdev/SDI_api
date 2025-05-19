namespace SDI_Api.Domain.Entities;

public class EstatePropertyDescription : BaseAuditableEntity
{
    public Guid EstatePropertyId { get; set; }

    public EstateProperty EstateProperty { get; set; } = default!;
}
