namespace SDI_Api.Application.MemberProfile.Commands;

public class ChangeUserPasswordCommand : IRequest<Unit>
{
    public ChangePasswordDto PasswordData { get; set; } = new ChangePasswordDto();
}

public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public ChangeUserPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
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

        var changePasswordResult = await _userManager.ChangePasswordAsync(
            user,
            request.PasswordData.OldPassword,
            request.PasswordData.NewPassword);

        if (!changePasswordResult.Succeeded)
        {
            var errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
            // Consider specific exceptions for "Invalid old password" vs "Password policy violation"
            throw new FluentValidation.ValidationException(errors); // Or a custom exception
        }

        return Unit.Value;
    }
}
