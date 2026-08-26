namespace SvadbeniSalon.Model.Requests;

public class DekoracijaInsertRequest
{
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cijena { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DekoracijaUpdateRequest
{
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cijena { get; set; }
    public bool IsActive { get; set; } = true;
}
