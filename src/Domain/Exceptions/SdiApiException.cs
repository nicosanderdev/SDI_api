namespace SDI_Api.Domain.Exceptions;

public abstract class SdiApiException : Exception
{
    public int StatusCode { get; }

    protected SdiApiException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class ConflictException : SdiApiException
{
    public ConflictException(string message) : base(message, 409) { }
}
