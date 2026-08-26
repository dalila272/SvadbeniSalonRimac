using SvadbeniSalon.Model.Enums;

namespace SvadbeniSalon.Model.Requests;

public class ArtikalInsertRequest
{
    public string Naziv { get; set; } = string.Empty;
    public TipArtikla Tip { get; set; }
    public decimal Cijena { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ArtikalUpdateRequest
{
    public string Naziv { get; set; } = string.Empty;
    public TipArtikla Tip { get; set; }
    public decimal Cijena { get; set; }
    public bool IsActive { get; set; } = true;
}
