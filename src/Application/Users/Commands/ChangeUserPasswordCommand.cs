using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Users;

namespace SDI_Api.Application.Users.Commands;

public class ChangeUserPasswordCommand : IRequest<Unit>
{
    public ChangePasswordDto PasswordData { get; set; } = new ChangePasswordDto();
}

public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, Unit>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public ChangeUserPasswordCommandHandler(ICurrentUserService currentUserService, IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<Unit> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var result = await _identityService.ChangePasswordAsync(userId.ToString(), request.PasswordData.OldPassword, request.PasswordData.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e));
            throw new FluentValidation.ValidationException(errors);
        }

        return Unit.Value;
    }
}
