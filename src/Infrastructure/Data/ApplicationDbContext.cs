using System.Reflection;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using SDI_Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SDI_Api.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<EstateProperty> EstateProperties => Set<EstateProperty>();
    public DbSet<EstatePropertyValues> EstatePropertyValues => Set<EstatePropertyValues>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<PropertyVideo> PropertyVideos => Set<PropertyVideo>();
    public DbSet<PropertyDocument> PropertyDocuments => Set<PropertyDocument>();
    public DbSet<MessageThread> MessageThreads => Set<MessageThread>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageRecipient> MessageRecipients => Set<MessageRecipient>();
    public DbSet<PropertyVisitLog> PropertyVisitLogs => Set<PropertyVisitLog>();
    public DbSet<PropertyMessageLog> PropertyMessageLogs => Set<PropertyMessageLog>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberSubscription> MemberSubscriptions => Set<MemberSubscription>();
    public DbSet<RecoveryCode> RecoveryCodes => Set<RecoveryCode>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<BillingHistory> BillingHistories => Set<BillingHistory>();
    public DbSet<Usage> Usages => Set<Usage>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); // If you use IEntityTypeConfiguration

        // Example explicit configuration if not using separate config files:
        builder.Entity<EstateProperty>(entity =>
        {
            entity.HasMany(e => e.PropertyImages)
                .WithOne(pi => pi.EstateProperty)
                .HasForeignKey(pi => pi.EstatePropertyId)
                .OnDelete(DeleteBehavior.Cascade); // Or Restrict, SetNull

            entity.HasMany(e => e.PropertyVideos)
                .WithOne(pv => pv.EstateProperty)
                .HasForeignKey(pi => pi.EstatePropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.PropertyDocuments)
                .WithOne(pv => pv.EstateProperty)
                .HasForeignKey(pi => pi.EstatePropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.EstatePropertyValues)
                .WithOne(pd => pd.EstateProperty)
                .HasForeignKey(pd => pd.EstatePropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EstatePropertyAmenity>(entity =>
        {
            entity.HasKey(ea => new { ea.EstatePropertyId, ea.AmenityId });

            entity.HasOne(ea => ea.EstateProperty)
                .WithMany(e => e.EstatePropertyAmenities)
                .HasForeignKey(ea => ea.EstatePropertyId);

            entity.HasOne(ea => ea.Amenity)
                .WithMany(a => a.EstatePropertyAmenities)
                .HasForeignKey(ea => ea.AmenityId);
        });

        builder.Entity<PropertyImage>(entity =>
        {
            // PropertyImage is owned by EstateProperty
        });
        
        builder.Entity<PropertyVideo>(entity =>
        {
            // PropertyVideo is owned by EstateProperty
        });

        builder.Entity<EstatePropertyValues>(entity =>
        {
            // EstatePropertyDescription is owned by EstateProperty
        });

        builder.Entity<Amenity>(entity =>
        {
            // Aminities is owned by EstateProperty
        });
        
        // Consider adding indexes to PropertyVisitLog.VisitedOnUtc, PropertyVisitLog.PropertyId,
        // PropertyMessageLog.SentOnUtc, PropertyMessageLog.PropertyId for performance.
        builder.Entity<PropertyVisitLog>(entity =>
        {
            entity.HasIndex(e => e.VisitedOnUtc);
            entity.HasIndex(e => e.PropertyId);
            entity.HasOne(e => e.Property).WithMany().HasForeignKey(e => e.PropertyId).IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade); // Or Restrict
        });

        builder.Entity<PropertyMessageLog>(entity =>
        {
            entity.HasIndex(e => e.SentOnUtc);
            entity.HasIndex(e => e.PropertyId);
            entity.HasOne(e => e.Property).WithMany().HasForeignKey(e => e.PropertyId).IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade); // Or Restrict
        });

        builder.Entity<MessageThread>(entity =>
        {
            entity.HasMany(t => t.Messages)
                .WithOne(m => m.Thread)
                .HasForeignKey(m => m.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(t => t.LastMessageAtUtc);
            entity.HasIndex(t => t.PropertyId);
        });

        builder.Entity<Message>(entity =>
        {
            entity.HasOne(m => m.InReplyToMessage)
                .WithMany() // A message doesn't have a collection of "replies to this"
                .HasForeignKey(m => m.InReplyToMessageId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull); // If original message is deleted, replies don't point to it.

            entity.HasMany(m => m.MessageRecipients)
                .WithOne(mr => mr.Message)
                .HasForeignKey(mr => mr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(m => m.CreatedAtUtc);
        });

        builder.Entity<MessageRecipient>(entity =>
        {
            entity.HasKey(mr => mr.Id); 
            entity.HasIndex(mr => new { mr.RecipientId, mr.IsRead, mr.IsArchived, mr.IsDeleted }); // For inbox counts
            entity.HasIndex(mr => new { mr.RecipientId, mr.IsStarred, mr.IsDeleted }); // For starred counts
        });
        
        builder.Entity<Member>(entity =>
        {
            entity.HasKey(ms => ms.Id);

            entity.HasIndex(m => m.UserId).IsUnique();

            entity.HasOne(m => m.MemberSubscription)
                .WithOne(ms => ms.Member)
                .HasForeignKey<MemberSubscription>(ms => ms.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MemberSubscription>(entity =>
        {
            entity.HasKey(ms => ms.Id);
            
            entity.HasIndex(ms => ms.MemberId).IsUnique();
        });
        
        builder.Entity<Favorite>(entity =>
        {
            entity.HasKey(f => new { f.MemberId, f.EstatePropertyId });
            
            entity.HasOne(f => f.Member)
                .WithMany(m => m.Favorites)
                .HasForeignKey(f => f.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.EstateProperty)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.EstatePropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(f => new { f.MemberId, f.EstatePropertyId }).IsUnique();
        });
        
        builder.Entity<RecoveryCode>(entity =>
        {
            entity.HasIndex(rc => rc.Code).IsUnique();
            entity.HasIndex(rc => rc.UserId);
        });
        
        // Subscription feature entities
        builder.Entity<Plan>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Key).IsUnique();
            entity.HasIndex(p => p.IsActive);
            
            entity.HasMany(p => p.Subscriptions)
                .WithOne(s => s.Plan)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        builder.Entity<Company>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.BillingContactUserId);
            entity.HasIndex(c => c.Name);
            
            entity.HasMany(c => c.UserCompanies)
                .WithOne(uc => uc.Company)
                .HasForeignKey(uc => uc.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<UserCompany>(entity =>
        {
            entity.HasKey(uc => uc.Id);
            entity.HasIndex(uc => new { uc.MemberId, uc.CompanyId }).IsUnique();
            entity.HasIndex(uc => uc.MemberId);
            entity.HasIndex(uc => uc.CompanyId);
        });
        
        builder.Entity<Subscription>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => new { s.OwnerType, s.OwnerId });
            entity.HasIndex(s => s.PlanId);
            entity.HasIndex(s => s.Status);
            entity.HasIndex(s => s.ProviderSubscriptionId);
            
            // This is handled at the application level based on OwnerType
            // No explicit FK constraint is set to allow flexibility
            entity.HasOne(s => s.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(s => s.BillingHistories)
                .WithOne(bh => bh.Subscription)
                .HasForeignKey(bh => bh.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<BillingHistory>(entity =>
        {
            entity.HasKey(bh => bh.Id);
            entity.HasIndex(bh => bh.SubscriptionId);
            entity.HasIndex(bh => bh.ProviderInvoiceId);
            entity.HasIndex(bh => bh.Status);
            
            entity.HasOne(bh => bh.Subscription)
                .WithMany(s => s.BillingHistories)
                .HasForeignKey(bh => bh.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<Usage>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => new { u.OwnerType, u.OwnerId, u.SnapshotAt });
            entity.HasIndex(u => u.SnapshotAt);
        });
        
        builder.Entity<WebhookEvent>(entity =>
        {
            entity.HasKey(we => we.Id);
            entity.HasIndex(we => we.ProviderEventId).IsUnique();
            entity.HasIndex(we => we.EventType);
            entity.HasIndex(we => we.Processed);
        });
    }
}
