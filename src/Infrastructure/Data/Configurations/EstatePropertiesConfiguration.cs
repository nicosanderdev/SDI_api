using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Infrastructure.Data.Configurations;

public class EstatePropertiesConfiguration : IEntityTypeConfiguration<EstateProperty>
{
    public void Configure(EntityTypeBuilder<EstateProperty> builder)
    {
        builder
            .HasOne(e => e.FeaturedDescription)
            .WithMany()
            .HasForeignKey(e => e.FeaturedDescriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
