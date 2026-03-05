using System.Text.Json.Serialization;

namespace API.Models;

/// <summary>
/// Payload para cadastro de um novo carrinho (agrupamento lógico)
/// </summary>
public class CreateCartPayload
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Payload para atualização de um carrinho
/// </summary>
public class UpdateCartPayload
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
}
