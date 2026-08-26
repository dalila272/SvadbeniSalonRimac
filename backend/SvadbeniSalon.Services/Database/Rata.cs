using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SvadbeniSalon.Services.Database;

public class Rata
{
    [Key]
    public int Id { get; set; }

    public int SvadbaId { get; set; }

    [ForeignKey(nameof(SvadbaId))]
    public Svadba Svadba { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Iznos { get; set; }

    public DateTime DatumUplate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
