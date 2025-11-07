namespace SDI_Api.Domain.Entities;

public class MemberSubscription : BaseAuditableEntity
{
    public bool isActive { get; set; } = false;
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public MemberSubscription()
    {
        Id = Guid.NewGuid();
    }
}
