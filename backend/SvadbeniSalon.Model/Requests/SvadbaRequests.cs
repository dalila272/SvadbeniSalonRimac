namespace SvadbeniSalon.Model.Requests;

public class SvadbaInsertRequest
{
    public int? UserId { get; set; }

    public int PonudaId { get; set; }
    public DateTime DatumSvadbe { get; set; }
    public TimeSpan Vrijeme { get; set; }
    public int BrojGostiju { get; set; }
    public int BrojRata { get; set; } = 1;
    public string? Napomena { get; set; }
}

public class SvadbaUpdateRequest
{
    public int PonudaId { get; set; }
    public DateTime DatumSvadbe { get; set; }
    public TimeSpan Vrijeme { get; set; }
    public int BrojGostiju { get; set; }
    public int BrojRata { get; set; } = 1;
    public string? Napomena { get; set; }
}
