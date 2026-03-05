using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

/// <summary>
/// Representa um carrinho - agrupamento lógico de bandejas
/// Um carrinho pode conter múltiplas bandejas
/// Estrutura reflete exatamente as colunas da tabela 'carrinhos' no PostgreSQL
/// </summary>
public class Cart
{
    /// <summary>
    /// ID do carrinho (gerado automaticamente via sequence)
    /// </summary>
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    /// <summary>
    /// Nome do carrinho (ex: "Carrinho Setor A", "Linha 1")
    /// Tipo no banco: varchar(100) NOT NULL
    /// </summary>
    [Column("nome")]
    [MaxLength(100)]
    [Required]
    public string nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição adicional do carrinho
    /// Tipo no banco: text NULLABLE
    /// </summary>
    [Column("descricao")]
    public string? descricao { get; set; }

    /// <summary>
    /// Indica se o carrinho está ativo
    /// Tipo no banco: bool NOT NULL DEFAULT true
    /// </summary>
    [Column("ativo")]
    public bool ativo { get; set; } = true;

    /// <summary>
    /// Data de criação do carrinho
    /// Tipo no banco: timestamptz DEFAULT CURRENT_TIMESTAMP
    /// </summary>
    [Column("data_criacao")]
    public DateTime? data_criacao { get; set; }

    /// <summary>
    /// Data da última atualização
    /// Tipo no banco: timestamptz NULLABLE
    /// </summary>
    [Column("data_atualizacao")]
    public DateTime? data_atualizacao { get; set; }

    /// <summary>
    /// Coleção de bandejas atribuídas a este carrinho (relacionamento 1:N)
    /// Um carrinho pode ter múltiplas bandejas
    /// </summary>
    public ICollection<Tray> Trays { get; set; } = new List<Tray>();
}
