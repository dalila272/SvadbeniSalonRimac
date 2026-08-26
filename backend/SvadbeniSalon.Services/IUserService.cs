using SvadbeniSalon.Model.Access;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;

namespace SvadbeniSalon.Services
{
    public interface IUserService : IBaseCRUDService<UserResponse, UserSearch, UserInsertRequest, UserUpdateRequest>
    {
        Task<UserSensitveResponse?> GetByUsernameAsync(string username);
        Task<UserResponse?> GetWithRoleByIdAsync(int id);
        Task ChangePasswordAsync(UserPasswordChangeRequest request);
        Task AdminSetPasswordAsync(int userId, AdminSetPasswordRequest request);
        Task<PasswordResetCreated?> RequestPasswordResetAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task<UserResponse> UpdateOwnProfileAsync(int userId, UserUpdateRequest request);
    }

    public class PasswordResetCreated
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
