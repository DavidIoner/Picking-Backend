using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

/// <summary>
/// Representa uma bandeja - dispositivo físico identificado por MAC Address
/// Pode ser atribuída a um carrinho (agrupamento lógico)
/// Estrutura reflete as colunas da tabela 'bandejas' no PostgreSQL
/// </summary>
public class Tray
{
    /// <summary>
    /// ID da bandeja (gerado automaticamente)
    /// </summary>
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    /// <summary>
    /// Endereço MAC único da bandeja (identificador do dispositivo físico)
    /// Tipo no banco: bpchar(17) NOT NULL UNIQUE
    /// </summary>
    [Required]
    [Column("mac_address")]
    [MaxLength(17)]
    public string mac_address { get; set; } = string.Empty;

    /// <summary>
    /// ID do carrinho ao qual esta bandeja está atribuída (chave estrangeira, nullable)
    /// NULL = bandeja não atribuída a nenhum carrinho
    /// </summary>
    [Column("carrinho_id")]
    public int? carrinho_id { get; set; }

    /// <summary>
    /// Referência ao carrinho (navegação)
    /// </summary>
    [ForeignKey("carrinho_id")]
    public Cart? Cart { get; set; }

    /// <summary>
    /// Nome da bandeja (ex: "Bandeja 1", "Setor A - Posição 1")
    /// Tipo no banco: varchar(100) NULLABLE
    /// </summary>
    [Column("nome")]
    [MaxLength(100)]
    public string? nome { get; set; }

    /// <summary>
    /// Descrição adicional da bandeja
    /// Tipo no banco: text NULLABLE
    /// </summary>
    [Column("descricao")]
    public string? descricao { get; set; }

    /// <summary>
    /// Configuração dos blocos da bandeja (armazenado como JSONB)
    /// Tipo no banco: jsonb NULLABLE
    /// </summary>
    [Column("blocos")]
    public List<TrayBlock>? blocos { get; set; }

    /// <summary>
    /// Indica se a bandeja está ativa
    /// Tipo no banco: bool NOT NULL DEFAULT true
    /// </summary>
    [Column("ativo")]
    public bool ativo { get; set; } = true;

    /// <summary>
    /// Data de criação
    /// Tipo no banco: timestamptz DEFAULT CURRENT_TIMESTAMP
    /// </summary>
    [Column("data_criacao")]
    public DateTime? data_criacao { get; set; }

    /// <summary>
    /// Data de atualização
    /// Tipo no banco: timestamptz NULLABLE
    /// </summary>
    [Column("data_atualizacao")]
    public DateTime? data_atualizacao { get; set; }
}

public class TrayBlock
{
    public int blockId { get; set; }

    /// Linha inicial do bloco (0-2)
    public int startRow { get; set; }

    /// Linha final do bloco (0-2)
    public int endRow { get; set; }

    /// Coluna inicial do bloco (0-5)
    public int startColumn { get; set; }

    /// Coluna final do bloco (0-5)
    public int endColumn { get; set; }

    /// Nome/Label do produto ou área
    public string label { get; set; } = string.Empty;

    /// Cor do bloco para visualização (hex color)
    public string? color { get; set; }

    /// Índice do sensor no array de leituras (0-based)
    /// Exemplo: se sensorIndex = 2, este bloco usa o valor de readings[2]
    public int? sensorIndex { get; set; }

    /// Peso alvo de uma peça individual em kg
    /// Usado para calcular quantidade aproximada de peças no bloco
    public double? targetWeight { get; set; }
}
