using SvadbeniSalon.Model.SearchObjects;

namespace SvadbeniSalon.Model.SearchObjects;

public class PonudaSearchObject : BaseSearchObject
{
    public string? Naziv { get; set; }
    public int? MeniId { get; set; }
    public int? MuzicarId { get; set; }
    public int? DekoracijaId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IncludeInactive { get; set; }
}
