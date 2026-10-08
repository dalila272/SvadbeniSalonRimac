namespace SvadbeniSalon.Model.Responses;

public class RecenzijaResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string KorisnikIme { get; set; } = string.Empty;
    public int PonudaId { get; set; }
    public string PonudaNaziv { get; set; } = string.Empty;
    public int? SvadbaId { get; set; }
    public DateTime? SvadbaDatum { get; set; }
    public int Ocjena { get; set; }
    public string? Komentar { get; set; }
    public DateTime CreatedAt { get; set; }
}
