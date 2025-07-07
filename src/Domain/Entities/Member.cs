using System.ComponentModel.DataAnnotations;

namespace SDI_Api.Domain.Entities;

public class Member : BaseAuditableEntity
{
    [Required]
    public Guid UserId { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(2048)]
    public string? AvatarUrl { get; set; }

    // Address Fields
    [MaxLength(255)]
    public string? Street { get; set; }
    [MaxLength(255)]
    public string? Street2 { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(100)]
    public string? State { get; set; }
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    [MaxLength(100)]
    public string? Country { get; set; }

    public Member()
    {
        Id = Guid.NewGuid();
        Created = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }
}
