namespace SvadbeniSalon.Model.Requests;

public class DnevniSastanakInsertRequest
{
    public int? UserId { get; set; }
    public string? KontaktIme { get; set; }
    public DateTime DatumSastanka { get; set; }
    public string? Napomena { get; set; }
}

public class DnevniSastanakUpdateRequest
{
    public DateTime DatumSastanka { get; set; }
    public string? Napomena { get; set; }
}
