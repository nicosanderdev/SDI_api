namespace SDI_Api.Domain.Entities;

public class EstateProperty : BaseAuditableEntity
{
    public List<EstatePropertyDescription> EstatePropertyDescriptions { get; set; } = new();
    
    public Guid? FeaturedDescriptionId  { get; set; }
    
    public EstatePropertyDescription? FeaturedDescription { get; set; }
}
