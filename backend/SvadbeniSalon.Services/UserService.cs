using SvadbeniSalon.Common.Services.CryptoService;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services.Database;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SvadbeniSalon.Model.Constants;

namespace SvadbeniSalon.Services
{
    public class UserService : BaseCRUDService<User, UserResponse, UserSearch, UserInsertRequest, UserUpdateRequest>, IUserService
    {
        private readonly ICryptoService _cryptoService;
        private readonly IAuthenticatedUserAccessor _userAccessor;
        private readonly IValidator<UserPasswordChangeRequest> _passwordChangeValidator;
        private readonly IValidator<AdminSetPasswordRequest> _adminSetPasswordValidator;

        public UserService(
            SvadbeniSalonDbContext dbContext,
            MapsterMapper.IMapper mapper,
            IValidator<UserInsertRequest> insertValidator,
            IValidator<UserUpdateRequest> updateValidator,
            IValidator<UserPasswordChangeRequest> passwordChangeValidator,
            IValidator<AdminSetPasswordRequest> adminSetPasswordValidator,
            ICryptoService cryptoService,
            IAuthenticatedUserAccessor userAccessor)
            : base(dbContext, mapper, insertValidator, updateValidator)
        {
            _cryptoService = cryptoService;
            _userAccessor = userAccessor;
            _passwordChangeValidator = passwordChangeValidator;
            _adminSetPasswordValidator = adminSetPasswordValidator;
        }

        protected override async Task<IQueryable<User>> IncludeRelatedEntitiesAsync(
            UserSearch? search,
            IQueryable<User> query = null!)
        {
            query ??= _dbContext.Set<User>();
            return query
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();
        }

        protected override IQueryable<User> ApplyFilters(IQueryable<User> query, UserSearch? search)
        {
            if (search != null)
            {
                if (!string.IsNullOrWhiteSpace(search.Email))
                {
                    var email = search.Email.Trim().ToLower();
                    query = query.Where(u => u.Email.ToLower().Contains(email));
                }

                if (!string.IsNullOrWhiteSpace(search.Username))
                {
                    var username = search.Username.Trim().ToLower();
                    query = query.Where(u => u.Username.ToLower().Contains(username));
                }

                if (!string.IsNullOrWhiteSpace(search.Name))
                {
                    var name = search.Name.Trim().ToLower();
                    query = query.Where(u =>
                        u.FirstName.ToLower().Contains(name) || u.LastName.ToLower().Contains(name));
                }

                if (search.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == search.IsActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(search.RoleName))
                {
                    var role = search.RoleName.Trim();
                    query = query.Where(u =>
                        u.UserRoles.Any(ur => ur.Role.Name == role));
                }
            }

            return query;
        }

        protected override User MapInsertRequestToEntity(UserInsertRequest request)
        {
            var entity = base.MapInsertRequestToEntity(request);

            var salt = _cryptoService.GenerateSlat();
            entity.PasswordSalt = salt;
            entity.PasswordHash = _cryptoService.GenerateHash(request.Password, salt);

            return entity;
        }

        public override async Task<UserResponse> InsertAsync(UserInsertRequest request)
        {
            await _insertValidator.ValidateAndThrowAsync(request);

            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new ClientException($"Email '{request.Email}' je već u upotrebi.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new ClientException($"Korisničko ime '{request.Username}' je već u upotrebi.");
            }

            var roleName = string.IsNullOrWhiteSpace(request.Role) ? RoleNames.Zaposlenik : request.Role.Trim();
            if (roleName is not (RoleNames.Admin or RoleNames.Zaposlenik or RoleNames.Customer))
            {
                throw new ClientException(
                    $"Uloga '{roleName}' nije dozvoljena. Dozvoljene: {RoleNames.Admin}, {RoleNames.Zaposlenik}, {RoleNames.Customer}.");
            }

            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                throw new ClientException($"Uloga '{roleName}' ne postoji. Pokreni migracije baze.");
            }

            var entity = MapInsertRequestToEntity(request);
            entity.CreatedAt = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                _dbContext.Users.Add(entity);
                await _dbContext.SaveChangesAsync();

                _dbContext.UserRoles.Add(new UserRole
                {
                    UserId = entity.Id,
                    RoleId = role.Id,
                    DateAssigned = DateTime.UtcNow
                });
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return (await GetWithRoleByIdAsync(entity.Id))!;
        }

        public override async Task<UserResponse> UpdateAsync(int id, UserUpdateRequest request)
        {
            await _updateValidator.ValidateAndThrowAsync(request);

            var entity = await _dbContext.Users.FindAsync(id);
            if (entity == null)
            {
                throw new NotFoundException($"User with id {id} not found.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
            {
                throw new ClientException($"Email '{request.Email}' je već u upotrebi.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username && u.Id != id))
            {
                throw new ClientException($"Korisničko ime '{request.Username}' je već u upotrebi.");
            }

            ImageContentValidator.EnsureValidImageBase64(request.ProfileImageBase64);
            MapUpdateRequestToEntity(request, entity);

            _dbContext.Users.Update(entity);
            await _dbContext.SaveChangesAsync();

            return (await GetWithRoleByIdAsync(id))!;
        }

        public override async Task DeleteAsync(int id)
        {
            var entity = await _dbContext.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (entity == null)
            {
                throw new NotFoundException($"User with id {id} not found.");
            }

            _dbContext.UserRoles.RemoveRange(entity.UserRoles);
            _dbContext.Users.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public override async Task<PageResult<UserResponse>> GetAllAsync(UserSearch? search = null)
        {
            search ??= new UserSearch();
            search.IncludeTotalCount ??= true;
            if (string.IsNullOrWhiteSpace(search.SortBy))
            {
                search.SortBy = "Id desc";
            }

            var query = await IncludeRelatedEntitiesAsync(search, _dbContext.Set<User>().AsQueryable());
            return await ExecutePagedAsync(query, search, u => MapUser(u, includeProfileImage: false));
        }

        public override async Task<UserResponse> GetByIdAsync(int id)
        {
            var user = await GetWithRoleByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException($"User with id {id} not found.");
            }

            return user;
        }

        public async Task<UserSensitveResponse?> GetByUsernameAsync(string username)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            UserSensitveResponse? response = null;

            if (user != null)
            {
                response = _mapper.Map<UserSensitveResponse>(user);
                response.Role = user.UserRoles.FirstOrDefault()?.Role.Name;
            }

            return response;
        }

        public async Task<UserResponse?> GetWithRoleByIdAsync(int id)
        {
            var user = await _dbContext.Users
               .AsNoTracking()
               .Include(u => u.UserRoles)
               .ThenInclude(ur => ur.Role)
               .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return null;
            }

            return MapUser(user);
        }

        public async Task ChangePasswordAsync(UserPasswordChangeRequest request)
        {
            await _passwordChangeValidator.ValidateAndThrowAsync(request);

            var currentUserId = _userAccessor.GetUserId()
                ?? throw new UnauthorizedAccessException("Niste prijavljeni.");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == currentUserId)
                ?? throw new NotFoundException($"User with id {currentUserId} not found.");

            if (!_cryptoService.Verify(user.PasswordHash, user.PasswordSalt, request.Password))
                throw new ClientException("Trenutna lozinka nije ispravna.");

            user.PasswordSalt = _cryptoService.GenerateSlat();
            user.PasswordHash = _cryptoService.GenerateHash(request.NewPassword, user.PasswordSalt);

            var refreshTokens = await _dbContext.RefreshTokens
                .Where(r => r.UserId == user.Id)
                .ToListAsync();
            _dbContext.RefreshTokens.RemoveRange(refreshTokens);

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AdminSetPasswordAsync(int userId, AdminSetPasswordRequest request)
        {
            await _adminSetPasswordValidator.ValidateAndThrowAsync(request);

            if (!_userAccessor.IsInRole(RoleNames.Admin))
                throw new UnauthorizedAccessException("Samo administrator može postaviti lozinku korisniku.");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new NotFoundException($"User with id {userId} not found.");

            user.PasswordSalt = _cryptoService.GenerateSlat();
            user.PasswordHash = _cryptoService.GenerateHash(request.NewPassword, user.PasswordSalt);

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<PasswordResetCreated?> RequestPasswordResetAsync(ForgotPasswordRequest request)
        {
            var key = request.EmailOrUsername?.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u =>
                    u.IsActive
                    && (u.Username == key || u.Email == key));

            if (user == null)
            {
                return null;
            }

            var existing = await _dbContext.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed)
                .ToListAsync();
            foreach (var token in existing)
            {
                token.IsUsed = true;
            }

            var code = GenerateSecureResetCode();
            var codeSalt = _cryptoService.GenerateSlat();
            var codeHash = _cryptoService.GenerateHash(code, codeSalt);
            _dbContext.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                CodeHash = codeHash,
                CodeSalt = codeSalt,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false,
            });
            await _dbContext.SaveChangesAsync();

            return new PasswordResetCreated
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                Code = code,
            };
        }

        private static string GenerateSecureResetCode()
        {
            var value = RandomNumberGenerator.GetInt32(100000, 1000000);
            return value.ToString();
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var key = request.EmailOrUsername?.Trim();
            var code = request.Code?.Trim();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(code))
            {
                throw new ClientException("Email/korisničko ime i kod su obavezni.");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            {
                throw new ClientException("Nova lozinka mora imati najmanje 8 karaktera.");
            }

            if (!request.NewPassword.Any(char.IsLetter) || !request.NewPassword.Any(char.IsDigit))
            {
                throw new ClientException("Nova lozinka mora sadržavati barem jedno slovo i jedan broj.");
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u =>
                    u.IsActive
                    && (u.Username == key || u.Email == key));

            if (user == null)
            {
                throw new ClientException("Neispravan kod ili korisnik.");
            }

            var now = DateTime.UtcNow;
            var candidates = await _dbContext.PasswordResetTokens
                .Where(t =>
                    t.UserId == user.Id
                    && !t.IsUsed
                    && t.ExpiresAt >= now)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var token = candidates.FirstOrDefault(t =>
                _cryptoService.Verify(t.CodeHash, t.CodeSalt, code));

            if (token == null)
            {
                throw new ClientException("Kod nije validan ili je istekao.");
            }

            token.IsUsed = true;
            user.PasswordSalt = _cryptoService.GenerateSlat();
            user.PasswordHash = _cryptoService.GenerateHash(request.NewPassword, user.PasswordSalt);

            var refreshTokens = await _dbContext.RefreshTokens
                .Where(r => r.UserId == user.Id)
                .ToListAsync();
            _dbContext.RefreshTokens.RemoveRange(refreshTokens);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserResponse> UpdateOwnProfileAsync(int userId, UserUpdateRequest request)
        {
            var entity = await _dbContext.Users.FindAsync(userId)
                ?? throw new NotFoundException($"User with id {userId} not found.");

            request.IsActive = entity.IsActive;
            ImageContentValidator.EnsureValidImageBase64(request.ProfileImageBase64);
            return await UpdateAsync(userId, request);
        }

        private UserResponse MapUser(User user, bool includeProfileImage = true)
        {
            var response = _mapper.Map<UserResponse>(user);
            if (!includeProfileImage)
            {
                response.ProfileImageBase64 = null;
            }
            response.Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? string.Empty;
            return response;
        }
    }
}
