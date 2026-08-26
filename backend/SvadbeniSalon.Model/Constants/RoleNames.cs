namespace SvadbeniSalon.Model.Constants;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
    public const string Zaposlenik = "Zaposlenik";

    public const string AdminOrZaposlenik = Admin + "," + Zaposlenik;
}
