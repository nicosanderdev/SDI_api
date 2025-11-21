using System.ComponentModel.DataAnnotations;
using SDI_Api.Domain.Constants;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Domain.Entities;

public class UserCompany : BaseEntity
{
    [Required]
    public Guid MemberId { get; set; }
    
    [Required]
    public Guid CompanyId { get; set; }
    
    [Required]
    public UserCompanyRole Role { get; set; }
    
    [Required]
    public Guid AddedBy { get; set; }
    
    [Required]
    public DateTimeOffset JoinedAt { get; set; }
    
    // Navigation properties
    public Company Company { get; set; } = null!;
    
    public UserCompany()
    {
        Id = Guid.NewGuid();
    }
}

