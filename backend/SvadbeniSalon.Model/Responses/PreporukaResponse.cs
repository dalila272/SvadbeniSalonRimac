namespace SvadbeniSalon.Model.Responses;

public class PreporukaResponse
{
    public int PonudaId { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cijena { get; set; }
    public double Score { get; set; }
    public string Razlog { get; set; } = string.Empty;
    public List<string> MatchingZanrovi { get; set; } = new();
}

public class UserInterestsResponse
{
    public List<int> ZanrIds { get; set; } = new();
    public List<ZanrResponse> Zanrovi { get; set; } = new();
}
