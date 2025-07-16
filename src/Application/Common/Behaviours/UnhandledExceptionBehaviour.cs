using Microsoft.Extensions.Logging;

namespace SDI_Api.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for request {RequestName} {@Request}", typeof(TRequest).Name, request);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Not Found: {RequestName} {@Request}", typeof(TRequest).Name, request);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access in request {RequestName} {@Request}", typeof(TRequest).Name, request);
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in request {RequestName} {@Request}", typeof(TRequest).Name, request);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {RequestName} {@Request}", typeof(TRequest).Name, request);
            throw;
        }
    }
}
