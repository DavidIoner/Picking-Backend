using System.Text.Json.Serialization;

namespace API.Models;

/// <summary>
/// Payload para criar uma nova bandeja (dispositivo físico)
/// </summary>
public class CreateTrayPayload
{
    [JsonPropertyName("macAddress")]
    public string MacAddress { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("cartId")]
    public int? CartId { get; set; }  // Opcional - bandeja pode ser criada sem estar atribuída
}

/// <summary>
/// Payload para atualizar a configuração da bandeja
/// </summary>
public class UpdateTrayPayload
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("cartId")]
    public int? CartId { get; set; }  // Pode ser NULL para desatribuir do carrinho

    [JsonPropertyName("blocks")]
    public List<TrayBlockPayload>? Blocks { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
}

/// <summary>
/// Payload legado - mantido para compatibilidade
/// </summary>
[Obsolete("Use UpdateTrayPayload instead")]
public class TrayConfigPayload
{
    [JsonPropertyName("cartId")]
    public int CartId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("blocks")]
    public List<TrayBlockPayload> Blocks { get; set; } = new List<TrayBlockPayload>();
}

public class TrayBlockPayload
{
    [JsonPropertyName("blockId")]
    public int BlockId { get; set; }

    [JsonPropertyName("startRow")]
    public int StartRow { get; set; }

    [JsonPropertyName("endRow")]
    public int EndRow { get; set; }

    [JsonPropertyName("startColumn")]
    public int StartColumn { get; set; }

    [JsonPropertyName("endColumn")]
    public int EndColumn { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("sensorIndex")]
    public int? SensorIndex { get; set; }

    [JsonPropertyName("targetWeight")]
    public double? TargetWeight { get; set; }
}
