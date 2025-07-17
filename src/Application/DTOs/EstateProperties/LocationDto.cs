using System.Text.Json.Serialization;

namespace SDI_Api.Application.DTOs.EstateProperties;


/// <summary>
/// Represents geographic coordinates. Maps to the 'location' object in the Zod schema.
/// </summary>
public class LocationDto
{
    [JsonPropertyName("lat")]
    public double Latitude { get; set; }
    [JsonPropertyName("lng")]
    public double Longitude { get; set; }
}
