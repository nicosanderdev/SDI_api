using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Common.Models;
using SDI_Api.Application.DTOs.Subscriptions;
using SDI_Api.Application.Subscriptions.Commands;
using SDI_Api.Application.Subscriptions.Queries;

namespace SDI_Api.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/subscriptions")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISender _sender;

    public SubscriptionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get current subscription for logged-in user or their company
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentSubscription()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        if (!Guid.TryParse(userIdValue, out var userGuid))
            throw new ArgumentException("Invalid user identifier format.");

        var query = new GetCurrentSubscriptionQuery { UserId = userGuid };
        var subscription = await _sender.Send(query);
        
        if (subscription == null)
            return NotFound();
        
        return Ok(subscription);
    }

    /// <summary>
    /// Create a checkout/payment session
    /// </summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(CheckoutResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequestDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        if (!Guid.TryParse(userIdValue, out var userGuid))
            throw new ArgumentException("Invalid user identifier format.");

        var command = new CreateCheckoutCommand
        {
            UserId = userGuid,
            Request = request
        };

        var response = await _sender.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Change plan for current subscription
    /// </summary>
    [HttpPost("change")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePlan([FromBody] ChangePlanRequestDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        if (!Guid.TryParse(userIdValue, out var userGuid))
            throw new ArgumentException("Invalid user identifier format.");

        var command = new ChangePlanCommand
        {
            UserId = userGuid,
            Request = request
        };

        var subscription = await _sender.Send(command);
        return Ok(subscription);
    }

    /// <summary>
    /// Cancel subscription
    /// </summary>
    [HttpPost("cancel")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionRequestDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        if (!Guid.TryParse(userIdValue, out var userGuid))
            throw new ArgumentException("Invalid user identifier format.");

        var command = new CancelSubscriptionCommand
        {
            UserId = userGuid,
            Request = request
        };

        var subscription = await _sender.Send(command);
        return Ok(subscription);
    }

    /// <summary>
    /// Get billing history for user/company
    /// </summary>
    [HttpGet("billing-history")]
    [ProducesResponseType(typeof(PaginatedResult<BillingHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBillingHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        if (!Guid.TryParse(userIdValue, out var userGuid))
            throw new ArgumentException("Invalid user identifier format.");

        var query = new GetBillingHistoryQuery
        {
            UserId = userGuid,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _sender.Send(query);
        return Ok(result);
    }
}

[ApiController]
[Route("api/plans")]
public class PlansController : ControllerBase
{
    private readonly ISender _sender;

    public PlansController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get available subscription plans and feature matrix
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(List<PlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans()
    {
        var query = new GetPlansQuery();
        var plans = await _sender.Send(query);
        return Ok(plans);
    }
}

[Authorize]
[ApiController]
[Route("api/companies/{companyId}/subscription")]
public class CompanySubscriptionsController : ControllerBase
{
    private readonly ISender _sender;

    public CompanySubscriptionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get subscription details for company (owner/admin only)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCompanySubscription([FromRoute] string companyId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue))
            throw new UnauthorizedAccessException("User identifier not found.");

        if (!Guid.TryParse(userIdValue, out var userGuid))
            throw new ArgumentException("Invalid user identifier format.");

        if (!Guid.TryParse(companyId, out var companyGuid))
            throw new ArgumentException("Invalid company ID format.");

        var query = new GetCompanySubscriptionQuery
        {
            CompanyId = companyGuid,
            UserId = userGuid
        };

        var subscription = await _sender.Send(query);
        
        if (subscription == null)
            return NotFound();
        
        return Ok(subscription);
    }
}

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/subscriptions")]
public class AdminSubscriptionsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminSubscriptionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// List all subscriptions in system (Admin only)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<SubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminSubscriptions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetAdminSubscriptionsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _sender.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create manual invoice / grant trial / adjust status (Admin only)
    /// </summary>
    [HttpPost("manual-invoice")]
    [ProducesResponseType(typeof(BillingHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateManualInvoice([FromBody] ManualInvoiceRequestDto request)
    {
        var command = new CreateManualInvoiceCommand
        {
            Request = request
        };

        var billingHistory = await _sender.Send(command);
        return Ok(billingHistory);
    }
}

[ApiController]
[Route("api/webhooks/payments")]
public class WebhooksController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(ISender sender, ILogger<WebhooksController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Handle payment provider webhook events
    /// </summary>
    [HttpPost]
    [AllowAnonymous] // Webhooks should be authenticated via signature, not user auth
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ProcessWebhook()
    {
        try
        {
            // Read raw body for signature verification
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // Get signature from headers (provider-specific)
            var signature = Request.Headers["X-Signature"].FirstOrDefault() ?? 
                           Request.Headers["Stripe-Signature"].FirstOrDefault() ?? 
                           string.Empty;

            // Parse webhook event
            using var document = JsonDocument.Parse(rawBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("id", out var idElement) || 
                !root.TryGetProperty("type", out var typeElement) ||
                !root.TryGetProperty("data", out var dataElement))
            {
                return BadRequest(new { error = "Invalid webhook payload structure" });
            }

            var webhookId = idElement.GetString() ?? string.Empty;
            var eventType = typeElement.GetString() ?? string.Empty;
            
            if (!dataElement.TryGetProperty("object", out var eventData))
            {
                return BadRequest(new { error = "Invalid webhook data structure" });
            }

            var command = new ProcessWebhookCommand
            {
                WebhookId = webhookId,
                EventType = eventType,
                EventData = eventData,
                Signature = signature,
                RawBody = rawBody
            };

            await _sender.Send(command);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return BadRequest(new { error = "Failed to process webhook" });
        }
    }
}

