namespace SvadbeniSalon.Model.Responses;

public class MeniResponse
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cijena { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ArtikalResponse> Artikli { get; set; } = new();
}

public class MuzicarResponse
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<int> ZanrIds { get; set; } = new();
    public List<ZanrResponse> Zanrovi { get; set; } = new();
}

public class DekoracijaResponse
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cijena { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ZanrResponse
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PonudaResponse
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string Opis { get; set; } = string.Empty;
    public decimal Cijena { get; set; }
    public int MeniId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public MeniResponse? Meni { get; set; }
    public List<MuzicarResponse> Muzicari { get; set; } = new();
    public List<DekoracijaResponse> Dekoracije { get; set; } = new();
}
