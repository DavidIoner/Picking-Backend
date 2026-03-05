using System.Text.Json.Serialization;

namespace API.Models;

public class ReadingPayload
{
    [JsonPropertyName("macaddress")]
    public string MacAddress { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("leituras")]
    public List<double> Readings { get; set; } = new List<double>();
}