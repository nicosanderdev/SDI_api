namespace SDI_Api.Application.DTOs.Subscriptions;

public class ChangePlanRequestDto
{
    public string PlanId { get; set; } = string.Empty;
    public bool Prorate { get; set; } = true;
}

