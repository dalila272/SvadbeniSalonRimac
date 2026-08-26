namespace SvadbeniSalon.Model.SearchObjects
{
    public class UserSearch : BaseSearchObject
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public string? RoleName { get; set; }
    }
}
