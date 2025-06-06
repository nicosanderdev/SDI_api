using SDI_Api.Application.Common.Interfaces;
using Sdi_Api.Application.DTOs.Profile;
using Sdi_Api.Application.MemberProfile;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.MemberProfile.Commands;

public class UpdateUserProfileCommand : IRequest<ProfileDataDto>
{
    public UpdateProfileDto ProfileUpdateData { get; set; } = new UpdateProfileDto();
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, ProfileDataDto>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateUserProfileCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService, IApplicationDbContext context, IMapper mapper)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProfileDataDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _identityService.FindUserByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(IUser), userId.ToString());
        }

        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);
        if (member == null)
        {
            throw new NotFoundException(nameof(Member), userId.ToString());
        }
        
        // Changes to member profile
        if (request.ProfileUpdateData.FirstName != null) member.FirstName = request.ProfileUpdateData.FirstName;
        if (request.ProfileUpdateData.LastName != null) member.LastName = request.ProfileUpdateData.LastName;
        if (request.ProfileUpdateData.Title != null) member.Title = request.ProfileUpdateData.Title;
        
        if (request.ProfileUpdateData.Address != null)
        {
            _mapper.Map(request.ProfileUpdateData.Address, member); // Assumes direct mapping from AddressDto to ApplicationUser address fields
        }

        // Changes to user
        if (!string.IsNullOrWhiteSpace(request.ProfileUpdateData.Email) && 
            !string.Equals(user.getUserEmail(), request.ProfileUpdateData.Email, StringComparison.OrdinalIgnoreCase))
        {
            // You might need to generate a token and confirm if EmailConfirmed is true
            // var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.ProfileUpdateData.Email);
            // var resultChangeEmail = await _userManager.ChangeEmailAsync(user, request.ProfileUpdateData.Email, token);
            // For simplicity, just setting it:
            var setResult = await _identityService.SetEmailAsync(userId.ToString(), request.ProfileUpdateData.Email);
            if (!setResult.Succeeded)
            {
                var errors = setResult.Errors.Select(e => e);     
                throw new Exception($"Profile update failed: {string.Join(", ", errors)}");
            }
        }
        
        if (request.ProfileUpdateData.Phone != null && 
            !string.Equals(user.getPhoneNumber(), request.ProfileUpdateData.Phone))
        {
            // Similar to email, changing phone might require confirmation
            // var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, request.ProfileUpdateData.Phone);
            // var resultChangePhone = await _userManager.ChangePhoneNumberAsync(user, request.ProfileUpdateData.Phone, token);
             var setResult = await _identityService.SetPhoneNumberAsync(userId.ToString(), request.ProfileUpdateData.Phone);
             if (!setResult.Succeeded)
             {
                 var errors = setResult.Errors.Select(e => e);     
                 throw new Exception($"Profile update failed: {string.Join(", ", errors)}");
             }
        }

        return _mapper.Map<ProfileDataDto>(user);
    }
}
