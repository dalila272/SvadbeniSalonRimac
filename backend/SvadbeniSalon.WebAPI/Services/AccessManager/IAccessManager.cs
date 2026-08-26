using SvadbeniSalon.Model.Access;

namespace SvadbeniSalon.WebAPI.Services.AccessManager
{
    public interface IAccessManager
    {
        Task<UserLoginResponse> LoginAsync(UserLoginRequest request);
        Task<UserLoginResponse> LoginWithRefreshTokenAsync(RefreshAccessTokenRequest request);
        Task LogoutAsync(int userId, string? refreshToken = null);
    }
}
