namespace SvadbeniSalon.Services.Database;

public class DekoracijaPonuda
{
    public int PonudaId { get; set; }
    public Ponuda Ponuda { get; set; } = null!;

    public int DekoracijaId { get; set; }
    public Dekoracija Dekoracija { get; set; } = null!;
}
