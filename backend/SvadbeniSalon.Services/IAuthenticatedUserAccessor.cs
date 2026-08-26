namespace SvadbeniSalon.Services;

public interface IAuthenticatedUserAccessor
{
    int? GetUserId();
    bool IsInRole(string role);
    bool IsSalonStaff();
}
