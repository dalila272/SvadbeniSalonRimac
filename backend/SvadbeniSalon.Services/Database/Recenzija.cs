using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SvadbeniSalon.Services.Database;

public class Recenzija
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public int PonudaId { get; set; }

    [ForeignKey(nameof(PonudaId))]
    public Ponuda Ponuda { get; set; } = null!;

    /// <summary>
    /// Optional: set when rating after a completed wedding; null for pre-wedding package ratings.
    /// </summary>
    public int? SvadbaId { get; set; }

    [ForeignKey(nameof(SvadbaId))]
    public Svadba? Svadba { get; set; }

    [Range(1, 5)]
    public int Ocjena { get; set; }

    [MaxLength(1000)]
    public string? Komentar { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
