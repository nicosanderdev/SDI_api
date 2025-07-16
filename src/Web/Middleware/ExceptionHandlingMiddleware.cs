using SDI_Api.Domain.Exceptions;

namespace SDI_Api.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, _logger);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
    {
        object errorResponse;
        int statusCode;

        switch (exception)
        {
            case SdiApiException apiEx:
                statusCode = apiEx.StatusCode;
                errorResponse = new { message = apiEx.Message };
                break;
            
            case NotFoundException notFound:
                statusCode = StatusCodes.Status404NotFound;
                errorResponse = new { message = notFound.Message };
                break;

            case UnauthorizedAccessException unauthorized:
                statusCode = StatusCodes.Status401Unauthorized;
                errorResponse = new { message = unauthorized.Message };
                break;

            case FluentValidation.ValidationException validation:
                statusCode = StatusCodes.Status400BadRequest;
                errorResponse = new
                {
                    message = "Validation failed.",
                    errors = validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                };
                break;

            case ArgumentException arg:
                statusCode = StatusCodes.Status400BadRequest;
                errorResponse = new { message = arg.Message };
                break;

            default:
                logger.LogError(exception, "Unhandled exception");
                statusCode = StatusCodes.Status400BadRequest;
                errorResponse = new { message = "An unexpected error occurred.", error = exception.Message };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(errorResponse);
    }
}
