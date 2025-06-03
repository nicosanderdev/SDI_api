using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Events;

namespace SDI_Api.Application.EstateProperties.Commands.Create;

public record CreateEstatePropertyCommand(CreateOrUpdateEstatePropertyDto Dto) : IRequest<Guid>;

public class CreateEstatePropertyCommandHandler : IRequestHandler<CreateEstatePropertyCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateEstatePropertyCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(CreateEstatePropertyCommand command, CancellationToken cancellationToken)
    {
        var request = command.Dto;
        var estateProperty = _mapper.Map<EstateProperty>(request);
        _context.EstateProperties.Add(estateProperty);
        estateProperty.AddDomainEvent(new EstatePropertyCreatedEvent(estateProperty));
        await _context.SaveChangesAsync(cancellationToken);
        return estateProperty.Id;
    }
}
