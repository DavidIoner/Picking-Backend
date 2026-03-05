using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class WeightReading
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }

    [Required]
    [MaxLength(17)]
    public string mac_address { get; set; } = string.Empty;

    [Required]
    public List<double> leituras { get; set; } = new List<double>();

    [Required]
    public DateTime timestamp_leitura { get; set; }
}