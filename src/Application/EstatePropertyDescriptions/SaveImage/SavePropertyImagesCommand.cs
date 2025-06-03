using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstatePropertyDescriptions.SaveImage;

public sealed record SavePropertyImagesCommand(ICollection<SavePropertyImageRequest> request) : IRequest;

public class SaveEstatePropertyImageCommandHandler : IRequestHandler<SavePropertyImagesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SaveEstatePropertyImageCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task Handle(SavePropertyImagesCommand command, CancellationToken cancellationToken)
    {
        var request = command.request;
        _context.PropertyImages.AddRange(_mapper.Map<ICollection<PropertyImage>>(request));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
