using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SvadbeniSalon.Services.Database;

public class Notifikacija
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Naslov { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Tekst { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Kind { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
