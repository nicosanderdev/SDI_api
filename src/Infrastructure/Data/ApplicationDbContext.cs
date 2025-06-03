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
    public DbSet<EstatePropertyDescription> EstatePropertyDescriptions => Set<EstatePropertyDescription>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<QandAMessageThread> QandAMessageThreads => Set<QandAMessageThread>();
    public DbSet<QandAMessage> QandAMessages => Set<QandAMessage>();
    public DbSet<PropertyVisitLog> PropertyVisitLogs => Set<PropertyVisitLog>();
    public DbSet<PropertyMessageLog> PropertyMessageLogs => Set<PropertyMessageLog>();

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

            entity.HasMany(e => e.EstatePropertyDescriptions)
                .WithOne(pd => pd.EstateProperty)
                .HasForeignKey(pd => pd.EstatePropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.MainImage)
                .WithMany() // A PropertyImage isn't exclusively a MainImage for multiple properties
                .HasForeignKey(e => e.MainImageId)
                .OnDelete(DeleteBehavior.SetNull) // If main image deleted, set FK to null
                .IsRequired(false);

            entity.HasOne(e => e.FeaturedDescription)
                .WithMany()
                .HasForeignKey(e => e.FeaturedDescriptionId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        builder.Entity<PropertyImage>(entity =>
        {
            // PropertyImage is owned by EstateProperty
        });

        builder.Entity<EstatePropertyDescription>(entity =>
        {
            // EstatePropertyDescription is owned by EstateProperty
        });
        
        // Consider adding indexes to PropertyVisitLog.VisitedOnUtc, PropertyVisitLog.PropertyId,
        // PropertyMessageLog.SentOnUtc, PropertyMessageLog.PropertyId for performance.
        builder.Entity<PropertyVisitLog>(entity =>
        {
            entity.HasIndex(e => e.VisitedOnUtc);
            entity.HasIndex(e => e.PropertyId);
            entity.HasOne(e => e.Property).WithMany().HasForeignKey(e => e.PropertyId).IsRequired(false).OnDelete(DeleteBehavior.Cascade); // Or Restrict
        });
        builder.Entity<PropertyMessageLog>(entity =>
        {
            entity.HasIndex(e => e.SentOnUtc);
            entity.HasIndex(e => e.PropertyId);
            entity.HasOne(e => e.Property).WithMany().HasForeignKey(e => e.PropertyId).IsRequired(false).OnDelete(DeleteBehavior.Cascade); // Or Restrict
        });
    }
}
