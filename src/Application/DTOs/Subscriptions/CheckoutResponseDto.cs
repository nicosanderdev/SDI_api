namespace SDI_Api.Application.DTOs.Subscriptions;

public class CheckoutResponseDto
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public string SessionId { get; set; } = string.Empty;
}

