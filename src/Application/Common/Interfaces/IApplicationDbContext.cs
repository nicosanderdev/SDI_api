using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<EstateProperty> EstateProperties { get; }
    
    DbSet<EstatePropertyDescription> EstatePropertyDescriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
