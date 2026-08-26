using System.ComponentModel.DataAnnotations;

namespace SvadbeniSalon.Services.Database;

public class Muzicar
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Naziv { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Opis { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<MuzicarZanr> MuzicarZanrovi { get; set; } = new List<MuzicarZanr>();

    public ICollection<MuzicarPonuda> MuzicariPonuda { get; set; } = new List<MuzicarPonuda>();
}
