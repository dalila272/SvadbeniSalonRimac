using SvadbeniSalon.Model.Enums;

namespace SvadbeniSalon.Model.SearchObjects;

public class ArtikalSearchObject : BaseSearchObject
{
    public string? Naziv { get; set; }
    public TipArtikla? Tip { get; set; }
    public bool? IsActive { get; set; }
}
