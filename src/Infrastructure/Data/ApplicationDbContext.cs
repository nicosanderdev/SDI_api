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
    public DbSet<MessageThread> MessageThreads => Set<MessageThread>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageRecipient> MessageRecipients => Set<MessageRecipient>();
    public DbSet<PropertyVisitLog> PropertyVisitLogs => Set<PropertyVisitLog>();
    public DbSet<PropertyMessageLog> PropertyMessageLogs => Set<PropertyMessageLog>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<RecoveryCode> RecoveryCodes => Set<RecoveryCode>();
    public DbSet<PropertyDocument> PropertyDocuments => Set<PropertyDocument>();

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

            entity.HasMany(e => e.EstatePropertyValues)
                .WithOne(pd => pd.EstateProperty)
                .HasForeignKey(pd => pd.EstatePropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PropertyImage>(entity =>
        {
            // PropertyImage is owned by EstateProperty
        });

        builder.Entity<EstatePropertyValues>(entity =>
        {
            // EstatePropertyDescription is owned by EstateProperty
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
            entity.HasKey(mr => mr.Id); // Explicitly define PK if not named 'Id' or '[ClassName]Id'
            // Or composite key: entity.HasKey(mr => new { mr.MessageId, mr.RecipientId });
            // If using composite key, then Id property is not needed on MessageRecipient.
            // For simplicity, single Guid Id is used here.

            entity.HasIndex(mr => new { mr.RecipientId, mr.IsRead, mr.IsArchived, mr.IsDeleted }); // For inbox counts
            entity.HasIndex(mr => new { mr.RecipientId, mr.IsStarred, mr.IsDeleted }); // For starred counts
        });
        
        builder.Entity<Member>(entity =>
        {
            entity.HasIndex(m => m.UserId).IsUnique();
        });
        
        builder.Entity<RecoveryCode>(entity =>
        {
            entity.HasIndex(rc => rc.Code).IsUnique();
            entity.HasIndex(rc => rc.UserId);
        });
    }
}
