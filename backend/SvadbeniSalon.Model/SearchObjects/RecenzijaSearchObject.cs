namespace SvadbeniSalon.Model.SearchObjects;

public class RecenzijaSearchObject : BaseSearchObject
{
    public int? UserId { get; set; }
    public int? PonudaId { get; set; }
    public int? SvadbaId { get; set; }
    public int? Ocjena { get; set; }
}
