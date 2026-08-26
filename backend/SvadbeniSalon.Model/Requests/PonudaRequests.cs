namespace SvadbeniSalon.Model.Requests;

public class PonudaInsertRequest
{
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cijena { get; set; }
    public int MeniId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<int> MuzicarIds { get; set; } = new();
    public List<int> DekoracijaIds { get; set; } = new();
}

public class PonudaUpdateRequest
{
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cijena { get; set; }
    public int MeniId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<int> MuzicarIds { get; set; } = new();
    public List<int> DekoracijaIds { get; set; } = new();
}
