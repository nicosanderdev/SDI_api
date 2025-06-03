using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<EstateProperty> EstateProperties { get; }
    DbSet<EstatePropertyDescription> EstatePropertyDescriptions { get; }
    DbSet<PropertyImage> PropertyImages { get; }
    DbSet<QandAMessageThread> QandAMessageThreads { get; }
    DbSet<QandAMessage> QandAMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
