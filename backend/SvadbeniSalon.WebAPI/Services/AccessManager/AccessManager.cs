using SvadbeniSalon.Common.Services.CryptoService;
using SvadbeniSalon.Model.Access;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Services;
using SvadbeniSalon.Services.Database;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SvadbeniSalon.WebAPI.Services.AccessManager
{
    public class AccessManager : IAccessManager
    {
        private readonly IUserService _userService;
        private readonly ICryptoService _cryptoService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly byte[] _jwtSecretKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtDurationMinutes;

        public AccessManager(
            IUserService userService,
            IConfiguration configuration,
            ICryptoService cryptoService,
            IRefreshTokenService refreshTokenService)
        {
            _userService = userService;
            _cryptoService = cryptoService;
            _refreshTokenService = refreshTokenService;

            _jwtSecretKey = Encoding.ASCII.GetBytes(
                configuration["JwtToken:SecretKey"]
                ?? throw new InvalidOperationException("JwtToken:SecretKey nije konfigurisan."));
            _jwtIssuer = configuration["JwtToken:Issuer"] ?? string.Empty;
            _jwtAudience = configuration["JwtToken:Audience"] ?? string.Empty;
            _jwtDurationMinutes = int.Parse(configuration["JwtToken:DurationInMinutes"] ?? "60");
        }

        public async Task<UserLoginResponse> LoginAsync(UserLoginRequest request)
        {
            var user = await _userService.GetByUsernameAsync(request.Username);


            if (user == null)
            {
                throw new ClientException($"Korisnik '{request.Username}' ne postoji.");
            }

            var validPassword = _cryptoService.Verify(user.PasswordHash, user.PasswordSalt, request.Password);
            if (!validPassword)
            {
                throw new ClientException("Pogrešni podaci za prijavu.");
            }

            if (!user.IsActive)
            {
                throw new ClientException("Korisnički račun nije aktivan.");
            }

            var accessToken = GenerateToken(user);
            var refreshTokenValue = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenService.InsertAsync(refreshToken);

            return new UserLoginResponse
            {
                Accesstoken = accessToken,
                Refreshtoken = refreshTokenValue,
                Role = user.Role ?? string.Empty
            };
        }

        public async Task<UserLoginResponse> LoginWithRefreshTokenAsync(RefreshAccessTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                throw new ClientException("Refresh token je obavezan.");
            }

            var refreshToken = await _refreshTokenService.GetStoredTokenAsync(request.RefreshToken);

            if (refreshToken == null)
            {
                throw new ClientException("Neispravan refresh token.");
            }

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new ClientException("Refresh token je istekao.");
            }

            var user = await _userService.GetWithRoleByIdAsync(refreshToken.UserId);

            if (user == null)
            {
                throw new ClientException("Korisnik nije pronađen.");
            }

            if (!user.IsActive)
            {
                throw new ClientException("Korisnik nije aktivan.");
            }

            var accessToken = GenerateToken(user);
            var refreshTokenValue = GenerateRefreshToken();

            var token = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenService.ReplaceUserRefreshTokenAsync(user.Id, token);

            return new UserLoginResponse
            {
                Accesstoken = accessToken,
                Refreshtoken = refreshTokenValue,
                Role = user.Role ?? string.Empty
            };

        }

        public async Task LogoutAsync(int userId, string? refreshToken = null)
        {
            await _refreshTokenService.DeleteAllUserRefreshTokensAsync(userId);
        }

        private string GenerateToken(UserResponse user)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimNames.Id, user.Id.ToString()),
                    new Claim(ClaimNames.FirstName, user.FirstName ?? string.Empty),
                    new Claim(ClaimNames.LastName, user.LastName ?? string.Empty),
                    new Claim(ClaimNames.Email, user.Email ?? string.Empty),
                    new Claim(ClaimNames.Role, user.Role ?? "user"),
                    new Claim(ClaimNames.IsActive, user.IsActive.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtDurationMinutes),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(_jwtSecretKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randombytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randombytes);
        }

       
    }
}
