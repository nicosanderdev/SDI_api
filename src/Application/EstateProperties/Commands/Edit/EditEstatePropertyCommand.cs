using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Events;

namespace SDI_Api.Application.EstateProperties.Commands.Edit;

public sealed record EditEstatePropertyCommand(EditEstatePropertyRequest request) : IRequest;

public class EditEstatePropertyCommandHandler : IRequestHandler<EditEstatePropertyCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public EditEstatePropertyCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(EditEstatePropertyCommand command, CancellationToken cancellationToken)
    {
        var request = command.request;
        var estateProperty = await _context.EstateProperties.FindAsync(new object[] { request.Id }, cancellationToken);
        estateProperty = _mapper.Map<EstateProperty>(request);
        estateProperty.AddDomainEvent(new EstatePropertyEditedEvent(estateProperty));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
