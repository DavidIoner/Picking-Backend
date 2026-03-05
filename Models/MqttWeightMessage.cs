using System.Text.Json.Serialization;

namespace API.Models;

/// <summary>
/// Modelo de mensagem recebida via MQTT no tópico devices/{mac}/weight
/// </summary>
public class MqttWeightMessage
{
    [JsonPropertyName("weights")]
    public List<double> Weights { get; set; } = new List<double>();

    [JsonPropertyName("timestamp")]
    public double Timestamp { get; set; }
}
