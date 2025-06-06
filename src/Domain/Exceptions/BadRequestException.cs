namespace SDI_Api.Domain.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string code)
        : base($"Colour \"{code}\" is unsupported.")
    {
    }
}
