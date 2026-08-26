using SvadbeniSalon.Services.Database;


namespace SvadbeniSalon.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> GetStoredTokenAsync(string refreshToken);
        Task InsertAsync(RefreshToken refreshToken);
        Task DeleteAllUserRefreshTokensAsync(int userId);
        Task ReplaceUserRefreshTokenAsync(int userId, RefreshToken newToken);
    }
}
