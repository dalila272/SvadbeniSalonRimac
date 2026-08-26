namespace SvadbeniSalon.Model.SearchObjects;

public class SvadbaSearchObject : BaseSearchObject
{
    public int? UserId { get; set; }
    public int? PonudaId { get; set; }
    public int? Status { get; set; }
    public DateTime? DatumOd { get; set; }
    public DateTime? DatumDo { get; set; }
}
