using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SvadbeniSalon.Model.Enums;

namespace SvadbeniSalon.Services.Database;

public class Svadba
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public int PonudaId { get; set; }

    [ForeignKey(nameof(PonudaId))]
    public Ponuda Ponuda { get; set; } = null!;

    public DateTime DatumSvadbe { get; set; }

    public TimeSpan Vrijeme { get; set; }

    public int BrojGostiju { get; set; }

    public int BrojRata { get; set; } = 1;

    public TerminStatus Status { get; set; } = TerminStatus.Pending;

    [MaxLength(1000)]
    public string? Napomena { get; set; }

    public int? StatusChangedByUserId { get; set; }

    [ForeignKey(nameof(StatusChangedByUserId))]
    public User? StatusChangedByUser { get; set; }

    public DateTime? StatusChangedAt { get; set; }

    [MaxLength(1000)]
    public string? StatusChangeReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Rata> Rate { get; set; } = new List<Rata>();

    public ICollection<Racun> Racuni { get; set; } = new List<Racun>();
}
