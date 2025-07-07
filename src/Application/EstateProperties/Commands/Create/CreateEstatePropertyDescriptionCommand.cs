using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Commands.Create;

public sealed record CreateEstatePropertyDescriptionCommand(CreatePropertyDescriptionRequest request) : IRequest;

public class CreateEstatePropertyDescriptionCommandHandler : IRequestHandler<CreateEstatePropertyDescriptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateEstatePropertyDescriptionCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task Handle(CreateEstatePropertyDescriptionCommand command, CancellationToken cancellationToken)
    {
        var request = command.request;
        var estatePropertyDescription = _mapper.Map<EstatePropertyValues>(request);
        _context.EstatePropertyValues.Add(estatePropertyDescription);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
