namespace SvadbeniSalon.Services.Database;

public class MeniArtikal
{
    public int MeniId { get; set; }
    public Meni Meni { get; set; } = null!;

    public int ArtikalId { get; set; }
    public Artikal Artikal { get; set; } = null!;
}
