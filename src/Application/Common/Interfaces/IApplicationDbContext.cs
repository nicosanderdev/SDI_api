using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<EstateProperty> EstateProperties { get; }
    DbSet<EstatePropertyValues> EstatePropertyValues { get; }
    DbSet<PropertyImage> PropertyImages { get; }
    DbSet<PropertyVideo> PropertyVideos { get; }
    DbSet<PropertyDocument> PropertyDocuments { get; }
    DbSet<MessageThread> MessageThreads { get; }
    DbSet<Message> Messages { get; }
    DbSet<MessageRecipient> MessageRecipients { get; }
    DbSet<PropertyVisitLog> PropertyVisitLogs { get; }
    DbSet<PropertyMessageLog> PropertyMessageLogs { get; }
    DbSet<Member> Members { get; }
    DbSet<MemberSubscription> MemberSubscriptions { get; }
    DbSet<RecoveryCode> RecoveryCodes { get; }
    DbSet<Amenity> Amenities { get; }
    DbSet<Favorite> Favorites { get; }
    DbSet<Plan> Plans { get; }
    DbSet<Company> Companies { get; }
    DbSet<UserCompany> UserCompanies { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<BillingHistory> BillingHistories { get; }
    DbSet<Usage> Usages { get; }
    DbSet<WebhookEvent> WebhookEvents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
