using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SvadbeniSalon.Services.Database;

public class Racun
{
    [Key]
    public int Id { get; set; }

    public int SvadbaId { get; set; }

    [ForeignKey(nameof(SvadbaId))]
    public Svadba Svadba { get; set; } = null!;

    [MaxLength(30)]
    public string BrojRacuna { get; set; } = string.Empty;

    public DateTime DatumIzdavanja { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UkupanIznos { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UplaceniIznos { get; set; }

    public int? KreiraoUserId { get; set; }

    [ForeignKey(nameof(KreiraoUserId))]
    public User? KreiraoUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
