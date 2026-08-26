using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace SvadbeniSalon.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly SvadbeniSalonDbContext _context;
        private readonly DbSet<RefreshToken> _refreshTokens;

        public RefreshTokenService(SvadbeniSalonDbContext context)
        {
            _context = context;
            _refreshTokens = _context.RefreshTokens;
        }

        public async Task<RefreshToken> GetStoredTokenAsync(string refreshToken)
        {
            var token = await _refreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null)
            {
                throw new ClientException("Refresh token nije pronađen.");
            }

            return token;
        }

        public async Task InsertAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public Task DeleteAllUserRefreshTokensAsync(int userId)
        {
            _refreshTokens.RemoveRange(_refreshTokens.Where(rt => rt.UserId == userId));
            return _context.SaveChangesAsync();
        }

        public async Task ReplaceUserRefreshTokenAsync(int userId, RefreshToken newToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _refreshTokens.RemoveRange(_refreshTokens.Where(rt => rt.UserId == userId));
                await _context.RefreshTokens.AddAsync(newToken);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
