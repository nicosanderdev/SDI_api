using Sdi_Api.Application.DTOs.Profile;
using Sdi_Api.Application.MemberProfile;

namespace SDI_Api.Application.MemberProfile.Commands;

public class UpdateUserProfileCommand : IRequest<ProfileDataDto>
{
    public UpdateProfileDto ProfileUpdateData { get; set; } = new UpdateProfileDto();
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, ProfileDataDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateUserProfileCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<ProfileDataDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserIdGuid();
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
        {
            throw new NotFoundException(nameof(ApplicationUser), userId.Value);
        }

        // Update basic properties
        if (request.ProfileUpdateData.FirstName != null) user.FirstName = request.ProfileUpdateData.FirstName;
        if (request.ProfileUpdateData.LastName != null) user.LastName = request.ProfileUpdateData.LastName;
        if (request.ProfileUpdateData.Title != null) user.Title = request.ProfileUpdateData.Title;

        // Update Address (if provided)
        if (request.ProfileUpdateData.Address != null)
        {
            _mapper.Map(request.ProfileUpdateData.Address, user); // Assumes direct mapping from AddressDto to ApplicationUser address fields
        }

        // Update Email if changed
        // Note: Changing a confirmed email typically involves a confirmation token process.
        // This example directly attempts to set it. Consider your Identity email confirmation strategy.
        if (!string.IsNullOrWhiteSpace(request.ProfileUpdateData.Email) && 
            !string.Equals(user.Email, request.ProfileUpdateData.Email, StringComparison.OrdinalIgnoreCase))
        {
            // You might need to generate a token and confirm if EmailConfirmed is true
            // var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.ProfileUpdateData.Email);
            // var resultChangeEmail = await _userManager.ChangeEmailAsync(user, request.ProfileUpdateData.Email, token);
            // For simplicity, just setting it:
            user.Email = request.ProfileUpdateData.Email;
            user.UserName = request.ProfileUpdateData.Email; // Often UserName is the Email
        }

        // Update Phone Number if changed
        if (request.ProfileUpdateData.Phone != null && 
            !string.Equals(user.PhoneNumber, request.ProfileUpdateData.Phone))
        {
            // Similar to email, changing phone might require confirmation
            // var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, request.ProfileUpdateData.Phone);
            // var resultChangePhone = await _userManager.ChangePhoneNumberAsync(user, request.ProfileUpdateData.Phone, token);
             var setResult = await _userManager.SetPhoneNumberAsync(user, request.ProfileUpdateData.Phone);
             if (!setResult.Succeeded) { /* Handle error */ }
        }
        
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            // Consolidate errors and throw or return a result object
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new Exception($"Profile update failed: {errors}"); // Or a custom exception
        }

        return _mapper.Map<ProfileDataDto>(user);
    }
}
