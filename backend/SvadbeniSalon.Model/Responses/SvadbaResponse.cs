namespace SvadbeniSalon.Model.Responses;

public class SvadbaResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string KorisnikIme { get; set; } = string.Empty;
    public int PonudaId { get; set; }
    public string PonudaNaziv { get; set; } = string.Empty;
    public DateTime DatumSvadbe { get; set; }
    public TimeSpan Vrijeme { get; set; }
    public int BrojGostiju { get; set; }
    public int BrojRata { get; set; }
    public int Status { get; set; }
    public string? Napomena { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int? StatusChangedByUserId { get; set; }
    public string? StatusChangedByName { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    public string? StatusChangeReason { get; set; }

    public decimal CijenaPonude { get; set; }

    public decimal UplaceniIznos { get; set; }

    public decimal PreostaliIznos { get; set; }

    public bool IsFullyPaid { get; set; }

    public int BrojEvidentiranihUplata { get; set; }
}
