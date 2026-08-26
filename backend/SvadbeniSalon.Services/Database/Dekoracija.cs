using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SvadbeniSalon.Services.Database;

public class Dekoracija
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Naziv { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Opis { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cijena { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<DekoracijaPonuda> DekoracijePonuda { get; set; } = new List<DekoracijaPonuda>();
}
