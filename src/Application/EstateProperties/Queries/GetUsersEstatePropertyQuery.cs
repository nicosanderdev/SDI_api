using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.EstateProperties.Queries;

public class GetUsersEstatePropertyQuery : IRequest<UsersEstatePropertyDto>
{
    public Guid? PropertyId { get; set; }
    public Guid? UserId { get; set; }
}

public class GetUsersEstatePropertyQueryHandler : IRequestHandler<GetUsersEstatePropertyQuery, UsersEstatePropertyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUsersEstatePropertyQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UsersEstatePropertyDto> Handle(GetUsersEstatePropertyQuery request, CancellationToken cancellationToken)
    {
        var estateProperty = await _context.EstateProperties
            .Where(ep => ep.Owner.UserId == request.UserId && ep.Id == request.PropertyId && !ep.IsDeleted)
            .Include(ep => ep.PropertyImages)
            .Include(ep => ep.EstatePropertyValues)
            .FirstOrDefaultAsync(cancellationToken);

        if (estateProperty == null)
            throw new NotFoundException(nameof(EstateProperty), request.PropertyId.ToString()!);
        
        var dto = _mapper.Map<UsersEstatePropertyDto>(estateProperty);
        _mapper.Map(estateProperty.EstatePropertyValues.FirstOrDefault(ep => ep.IsFeatured)!, dto);
        _mapper.Map(estateProperty.PropertyImages.Where(pi => !pi.IsDeleted), dto.Images);
        return dto;
    }
}
