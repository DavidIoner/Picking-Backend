namespace API.Options;

/// <summary>
/// Configurações do cliente MQTT
/// </summary>
public class MqttOptions
{
    public const string SectionName = "Mqtt";

    /// <summary>
    /// Endereço do broker MQTT (ex: localhost, broker.hivemq.com)
    /// </summary>
    public string Server { get; set; } = "broker.hivemq.com";

    /// <summary>
    /// Porta do broker MQTT (padrão: 1883)
    /// </summary>
    public int Port { get; set; } = 1883;

    /// <summary>
    /// ID do cliente MQTT
    /// </summary>
    public string ClientId { get; set; } = "cnh-api-subscriber";

    /// <summary>
    /// Usuário para autenticação (opcional)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Senha para autenticação (opcional)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Tópico para inscrição (padrão: devices/+/weight)
    /// </summary>
    public string Topic { get; set; } = "devices/+/weight";

    /// <summary>
    /// Habilitar/desabilitar cliente MQTT
    /// </summary>
    public bool Enabled { get; set; } = true;
}
