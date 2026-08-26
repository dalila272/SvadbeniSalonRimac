namespace SvadbeniSalon.Model.Requests;

public class RecenzijaInsertRequest
{
    public int PonudaId { get; set; }
    public int SvadbaId { get; set; }
    public int Ocjena { get; set; }
    public string? Komentar { get; set; }
}

public class RecenzijaUpdateRequest
{
    public int Ocjena { get; set; }
    public string? Komentar { get; set; }
}
