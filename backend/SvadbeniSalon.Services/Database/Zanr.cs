using System.ComponentModel.DataAnnotations;

namespace SvadbeniSalon.Services.Database;

public class Zanr
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Naziv { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MuzicarZanr> MuzicarZanrovi { get; set; } = new List<MuzicarZanr>();
}
