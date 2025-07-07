namespace SDI_Api.Application.DTOs.EstateProperties;


/// <summary>
/// Represents geographic coordinates. Maps to the 'location' object in the Zod schema.
/// </summary>
public class LocationDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}
