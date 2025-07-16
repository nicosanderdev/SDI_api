using SDI_Api.Application.Common.Interfaces;
using Sdi_Api.Application.DTOs.Profile;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.MemberProfile.Commands;

public class UpdateUserProfileCommand : IRequest<ProfileDataDto>
{
    public Guid? UserId { get; set; }
    public ProfileDataDto? UpdateProfileDto { get; set; }
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, ProfileDataDto>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateUserProfileCommandHandler(IIdentityService identityService, IApplicationDbContext context, IMapper mapper)
    {
        _identityService = identityService;
        _context = context;
        _mapper = mapper;
    }

    public async Task<ProfileDataDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId.ToString()!);
        if (user == null)
            throw new NotFoundException(nameof(IUser), request.UserId.ToString()!);

        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken);
        if (member == null)
            throw new NotFoundException(nameof(Member), request.UserId.ToString()!);
        
        // Changes to member profile
        if (request.UpdateProfileDto!.FirstName != null) member.FirstName = request.UpdateProfileDto.FirstName;
        if (request.UpdateProfileDto.LastName != null) member.LastName = request.UpdateProfileDto.LastName;
        if (request.UpdateProfileDto.Title != null) member.Title = request.UpdateProfileDto.Title;
        if (request.UpdateProfileDto.Address != null)
            _mapper.Map(request.UpdateProfileDto.Address, member);
        

        var profileDataDto = _mapper.Map<ProfileDataDto>(member);

        if (request.UpdateProfileDto.Phone == null || user.getPhoneNumber() == request.UpdateProfileDto.Phone)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map(user, profileDataDto);
        }

        var setResult = await _identityService.SetPhoneNumberAsync(request.UserId.ToString()!, request.UpdateProfileDto.Phone);
        if (!setResult.Succeeded)
            throw new Exception($"Profile update failed: {string.Join(", ", setResult.Errors)}");

        await _context.SaveChangesAsync(cancellationToken);
        profileDataDto.Phone = request.UpdateProfileDto.Phone;
        return profileDataDto;
    }
}
