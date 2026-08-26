namespace SvadbeniSalon.Model.Requests;

public class MeniInsertRequest
{
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cijena { get; set; }
    public bool IsActive { get; set; } = true;
    public List<int> HranaIds { get; set; } = new();
    public List<int> PiceIds { get; set; } = new();
}

public class MeniUpdateRequest
{
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cijena { get; set; }
    public bool IsActive { get; set; } = true;
    public List<int> HranaIds { get; set; } = new();
    public List<int> PiceIds { get; set; } = new();
}
