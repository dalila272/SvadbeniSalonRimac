namespace SvadbeniSalon.Model.Requests;

public class MuzicarInsertRequest
{
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<int> ZanrIds { get; set; } = new();
}

public class MuzicarUpdateRequest
{
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<int> ZanrIds { get; set; } = new();
}
