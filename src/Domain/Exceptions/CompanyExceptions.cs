namespace SDI_Api.Domain.Exceptions;

public class ForbiddenAccessException : SdiApiException
{
    public ForbiddenAccessException(string message) : base(message, 403) { }
}

public class CompanyNotFoundException : SdiApiException
{
    public CompanyNotFoundException(string message) : base(message, 404) { }
}

public class InvalidCompanyFilterException : SdiApiException
{
    public InvalidCompanyFilterException(string message) : base(message, 400) { }
}
