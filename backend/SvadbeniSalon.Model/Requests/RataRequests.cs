namespace SvadbeniSalon.Model.Requests;

public class RataInsertRequest
{
    public int SvadbaId { get; set; }
    public decimal Iznos { get; set; }
    public DateTime DatumUplate { get; set; }
}
