namespace SvadbeniSalon.Model.Requests;

public class ZanrInsertRequest
{
    public string Naziv { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class ZanrUpdateRequest
{
    public string Naziv { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
