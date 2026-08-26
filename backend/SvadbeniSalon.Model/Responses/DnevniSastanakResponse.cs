namespace SvadbeniSalon.Model.Responses;

public class DnevniSastanakResponse
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string KorisnikIme { get; set; } = string.Empty;
    public string? KontaktIme { get; set; }
    public DateTime DatumSastanka { get; set; }
    public int Status { get; set; }
    public string? Napomena { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int? StatusChangedByUserId { get; set; }
    public string? StatusChangedByName { get; set; }
    public DateTime? StatusChangedAt { get; set; }
    public string? StatusChangeReason { get; set; }
}
