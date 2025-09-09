using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Users;
using SDI_Api.Domain.Entities;
using NotFoundException = SDI_Api.Application.Common.Exceptions.NotFoundException;
using ValidationException = FluentValidation.ValidationException;

namespace SDI_Api.Application.Users.Commands;

public class RegisterUserCommand : IRequest<Result<UserDto>>
{
    public required RegisterUserDto RegisterUserDto { get; set; }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateProvider _emailTemplateProvider;
    private readonly IConfiguration _configuration;
    private readonly IEmailConfirmationTokenService _tokenService;

    public RegisterUserCommandHandler(IMapper mapper, 
        IIdentityService identityService, 
        IApplicationDbContext context, 
        IEmailService emailService, 
        IEmailTemplateProvider emailTemplateProvider, 
        IConfiguration configuration,
        IEmailConfirmationTokenService tokenService)
    {
        _mapper = mapper;
        _identityService = identityService;
        _context = context;
        _emailService = emailService;
        _emailTemplateProvider = emailTemplateProvider;
        _configuration = configuration;
        _tokenService = tokenService;
    }
    
    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var (result, userId) = await _identityService.CreateUserAsync(
            request.RegisterUserDto.Email,
            request.RegisterUserDto.Password,
            request.RegisterUserDto.FirstName,
            request.RegisterUserDto.LastName,
            cancellationToken);

        if (!result.Succeeded)
            return Result.Failure<UserDto>(result.Errors);
        
        var member = new Member()
        {
            UserId = Guid.Parse(userId),
            FirstName = request.RegisterUserDto.FirstName,
            LastName = request.RegisterUserDto.LastName,
            AvatarUrl = "https://placehold.co/150x150" 
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync(cancellationToken);
        
        var user = await _identityService.FindUserByIdAsync(userId);
        if (user is null)
            return Result.Failure<UserDto>( new List<string> { $"A user with the ID '{userId}' could not be found after creation." }); 
        
        // Send email for confirmation
        var confirmationLink = _configuration["AppUrls:ReactAppConfirmationUrl"];
        var token = _tokenService.GenerateToken(userId, user.getUserEmail()!);
        
        if (string.IsNullOrEmpty(confirmationLink))
            return Result.Failure<UserDto>( new List<string> { "Confirmation Link is not configured in appsettings.json." });
            
        var emailBody = _emailTemplateProvider.GetConfirmationEmailBody(confirmationLink+$"?token={token}");
        await _emailService.SendEmailAsync(
            toEmail: user.getUserEmail()!,
            subject: "Confirm Your Email Address",
            body: emailBody,
            true);
        
        var userDto = _mapper.Map<UserDto>(user);
        return Result.Success(userDto);
    }
}
