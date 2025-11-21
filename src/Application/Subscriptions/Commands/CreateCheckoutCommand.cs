using Microsoft.Extensions.Configuration;
using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Interfaces;
using SDI_Api.Application.DTOs.Subscriptions;
using SDI_Api.Domain.Entities;
using SDI_Api.Domain.Enums;
using NotFoundException = Ardalis.GuardClauses.NotFoundException;

namespace SDI_Api.Application.Subscriptions.Commands;

public class CreateCheckoutCommand : IRequest<CheckoutResponseDto>
{
    public Guid UserId { get; set; }
    public CheckoutRequestDto Request { get; set; } = null!;
}

public class CreateCheckoutCommandHandler : IRequestHandler<CreateCheckoutCommand, CheckoutResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public CreateCheckoutCommandHandler(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<CheckoutResponseDto> Handle(CreateCheckoutCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Request.PlanId, out var planId))
            throw new ArgumentException("Invalid plan ID format.");

        var plan = await _context.Plans.FindAsync(new object[] { planId }, cancellationToken);
        if (plan == null || !plan.IsActive)
            throw new NotFoundException(nameof(Plan), request.Request.PlanId);

        
        Guid ownerId;
        // OwnerType ownerType = OwnerType.Company;
        
        if (request.Request.IsCompanySubscription && !string.IsNullOrEmpty(request.Request.CompanyId))
        {
            if (!Guid.TryParse(request.Request.CompanyId, out var companyId))
                throw new ArgumentException("Invalid company ID format.");

            // Verify user has permission to create company subscription
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == request.UserId, cancellationToken);
            
            if (member == null)
                throw new NotFoundException(nameof(Member), request.UserId.ToString());

            var userCompany = await _context.UserCompanies
                .Where(uc => uc.MemberId == member.Id && uc.CompanyId == companyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (userCompany == null || (userCompany.Role != UserCompanyRole.Admin))
                throw new ForbiddenAccessException();

            ownerId = companyId;
        }
        else
        {
            ownerId = request.UserId;
        }

        // TODO: Integrate with payment provider (Stripe, PayPal, etc.)

        var checkoutUrl = _configuration["PaymentProvider:CheckoutUrl"] ?? "https://checkout.example.com/session";
        var sessionId = Guid.NewGuid().ToString();

        return new CheckoutResponseDto
        {
            CheckoutUrl = $"{checkoutUrl}/{sessionId}",
            SessionId = sessionId,
            ClientSecret = null // Set if using payment provider that requires client secret
        };
    }
}

