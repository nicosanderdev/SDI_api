using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Events;

namespace SDI_Api.Application.EstatePropertyDescriptions.Save;

public sealed record SaveEstatePropertyDescriptionCommand(SaveEstatePropertyDescriptionRequest request) : IRequest;

public class SaveEstatePropertyCommandHandler : IRequestHandler<SaveEstatePropertyDescriptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SaveEstatePropertyCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task Handle(SaveEstatePropertyDescriptionCommand command, CancellationToken cancellationToken)
    {
        var request = command.request;
        var estatePropertyDescription = _context.EstatePropertyDescriptions.Add(_mapper.Map<EstatePropertyDescription>(request)).Entity;
        estatePropertyDescription.AddDomainEvent(new EstatePropertyDescriptionCreatedEvent(estatePropertyDescription));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
