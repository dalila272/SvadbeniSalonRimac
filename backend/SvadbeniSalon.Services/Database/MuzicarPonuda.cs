namespace SvadbeniSalon.Services.Database;

public class MuzicarPonuda
{
    public int PonudaId { get; set; }
    public Ponuda Ponuda { get; set; } = null!;

    public int MuzicarId { get; set; }
    public Muzicar Muzicar { get; set; } = null!;
}
