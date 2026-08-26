namespace SvadbeniSalon.Model.Responses;

public class RacunResponse
{
    public int Id { get; set; }
    public int SvadbaId { get; set; }
    public string BrojRacuna { get; set; } = string.Empty;
    public DateTime DatumIzdavanja { get; set; }
    public decimal UkupanIznos { get; set; }
    public decimal UplaceniIznos { get; set; }
    public decimal PreostaliIznos => UkupanIznos - UplaceniIznos;
    public bool IsFullyPaid => PreostaliIznos <= 0;
    public string KorisnikIme { get; set; } = string.Empty;
    public string PonudaNaziv { get; set; } = string.Empty;
    public DateTime DatumSvadbe { get; set; }
    public DateTime CreatedAt { get; set; }
}
