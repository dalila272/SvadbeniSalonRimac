namespace SvadbeniSalon.Model.Access
{
    public class UserLoginResponse
    {
        public string Accesstoken { get; set; } = string.Empty;
        public string Refreshtoken { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
