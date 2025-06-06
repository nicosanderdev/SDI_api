using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;

namespace SDI_Api.Application.MemberProfile.Commands;

public class DeleteUserAndMemberCommand : IRequest
{
    public Guid UserId { get; set; }
}

public class DeleteUserAndMemberCommandHandler : IRequestHandler<DeleteUserAndMemberCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    
    public DeleteUserAndMemberCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task Handle(DeleteUserAndMemberCommand request, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User not found.", request.UserId.ToString());
        }
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == request.UserId);
        if (member == null)
        {
            throw new NotFoundException("Member not found.", request.UserId.ToString());
        }
        _context.Members.Remove(member);
        try 
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _identityService.DeleteUserAsync(request.UserId.ToString());
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DbUpdateConcurrencyException("Failed to delete member. The member may have been modified or deleted by another user.");
        }
    }
}
