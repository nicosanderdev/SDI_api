using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<EstateProperty> EstateProperties { get; }
    DbSet<EstatePropertyDescription> EstatePropertyDescriptions { get; }
    DbSet<PropertyImage> PropertyImages { get; }
    DbSet<MessageThread> MessageThreads { get; }
    DbSet<Message> Messages { get; }
    DbSet<MessageRecipient> MessageRecipients { get; }
    DbSet<PropertyVisitLog> PropertyVisitLogs { get; }
    DbSet<PropertyMessageLog> PropertyMessageLogs { get; }
    DbSet<Member> Members { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
