using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SvadbeniSalon.Model.Enums;

namespace SvadbeniSalon.Services.Database;

public class DnevniSastanak
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [MaxLength(200)]
    public string? KontaktIme { get; set; }

    public DateTime DatumSastanka { get; set; }

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
}
