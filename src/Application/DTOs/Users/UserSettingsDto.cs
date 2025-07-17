namespace SDI_Api.Application.DTOs.Users;

public class UserSettingsDto
{
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool NewListings { get; set; }
    public bool PriceDrops { get; set; }
    public bool StatusChanges { get; set; }
    public bool OpenHouses { get; set; }
    public bool MarketUpdates { get; set; }
    public bool Email { get; set; }
    public bool Push { get; set; }

    public UserSettingsDto(bool emailConfirmed, bool twoFactorEnabled)
    {
        EmailConfirmed = emailConfirmed;
        TwoFactorEnabled = twoFactorEnabled;
        NewListings = false;
        PriceDrops = false;
        StatusChanges = false;
        OpenHouses = false;
        MarketUpdates = false;
        Email = false;
        Push = false;
    }
}
