namespace SvadbeniSalon.Model.Responses;

public class RataResponse
{
    public int Id { get; set; }
    public int SvadbaId { get; set; }
    public decimal Iznos { get; set; }
    public DateTime DatumUplate { get; set; }
    public DateTime CreatedAt { get; set; }
}
