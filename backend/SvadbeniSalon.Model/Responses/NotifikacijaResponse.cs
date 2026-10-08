namespace SvadbeniSalon.Model.Responses;

public class NotifikacijaResponse
{
    public int Id { get; set; }
    public string Naslov { get; set; } = string.Empty;
    public string Tekst { get; set; } = string.Empty;
    public string? Kind { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UnreadCountResponse
{
    public int Count { get; set; }
}
