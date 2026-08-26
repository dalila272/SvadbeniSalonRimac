namespace SvadbeniSalon.Services.Database;

public class MuzicarZanr
{
    public int MuzicarId { get; set; }
    public Muzicar Muzicar { get; set; } = null!;

    public int ZanrId { get; set; }
    public Zanr Zanr { get; set; } = null!;
}
