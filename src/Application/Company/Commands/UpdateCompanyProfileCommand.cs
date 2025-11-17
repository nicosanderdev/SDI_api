using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Company;
using SDI_Api.Domain.Entities;
using NotFoundException = SDI_Api.Application.Common.Exceptions.NotFoundException;

namespace SDI_Api.Application.Company.Commands;

public class UpdateCompanyProfileCommand : IRequest<CompanyDto>
{
    public Guid UserId { get; set; }
    public required UpdateCompanyProfileDto ProfileData { get; set; }
}

public class UpdateCompanyProfileCommandHandler : IRequestHandler<UpdateCompanyProfileCommand, CompanyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateCompanyProfileCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyProfileCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .Where(m => m.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (member == null)
            throw new NotFoundException(nameof(Member), request.UserId.ToString());

        var userCompany = await _context.UserCompanies
            .Where(uc => uc.MemberId == member.Id)
            .Include(uc => uc.Company)
            .FirstOrDefaultAsync(cancellationToken);

        if (userCompany == null)
            throw new NotFoundException("Company", "User is not associated with a company.");

        var company = userCompany.Company;

        // Update company fields
        if (!string.IsNullOrWhiteSpace(request.ProfileData.Name))
            company.Name = request.ProfileData.Name;

        if (request.ProfileData.Description != null)
            company.Description = request.ProfileData.Description;

        if (request.ProfileData.Phone != null)
            company.Phone = request.ProfileData.Phone;

        // Update address fields
        if (request.ProfileData.Address != null)
        {
            company.Street = request.ProfileData.Address.Street;
            company.Street2 = request.ProfileData.Address.Street2;
            company.City = request.ProfileData.Address.City;
            company.State = request.ProfileData.Address.State;
            company.PostalCode = request.ProfileData.Address.PostalCode;
            company.Country = request.ProfileData.Address.Country;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CompanyDto>(company);
    }
}

