namespace SvadbeniSalon.Services.Database;

public class UserZanr
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ZanrId { get; set; }
    public Zanr Zanr { get; set; } = null!;
}
