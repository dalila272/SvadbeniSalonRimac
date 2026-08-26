using SvadbeniSalon.Model.Enums;

namespace SvadbeniSalon.Model.Responses;

public class ArtikalResponse
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public TipArtikla Tip { get; set; }
    public decimal Cijena { get; set; }
    public bool IsActive { get; set; }
}
