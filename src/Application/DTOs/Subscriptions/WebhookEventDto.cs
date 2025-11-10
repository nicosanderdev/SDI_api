using System.Text.Json;
using System.Text.Json.Serialization;

namespace SDI_Api.Application.DTOs.Subscriptions;

public class WebhookEventDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
    
    [JsonPropertyName("created")]
    public long Created { get; set; }
}

