using SvadbeniSalon.Model.Access;
using SvadbeniSalon.Model.Constants;
using SvadbeniSalon.Model.Messages;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Services;
using SvadbeniSalon.WebAPI.Services.AccessManager;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SvadbeniSalon.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AccessController : Controller
    {
        private readonly IAccessManager _accessManager;
        private readonly IUserService _userService;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IAuthenticatedUserAccessor _userAccessor;
        private readonly IValidator<UserRegisterRequest> _registerValidator;

        public AccessController(
            IAccessManager accessManager,
            IUserService userService,
            INotificationPublisher notificationPublisher,
            IAuthenticatedUserAccessor userAccessor,
            IValidator<UserRegisterRequest> registerValidator)
        {
            _accessManager = accessManager;
            _userService = userService;
            _notificationPublisher = notificationPublisher;
            _userAccessor = userAccessor;
            _registerValidator = registerValidator;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] UserLoginRequest request)
        {
            var result = await _accessManager.LoginAsync(request);
            return Ok(result);
        }

        [HttpPost("LoginWithRefreshToken")]
        [AllowAnonymous]
        public async Task<ActionResult> LoginWithRefreshToken([FromBody] RefreshAccessTokenRequest request)
        {
            var result = await _accessManager.LoginWithRefreshTokenAsync(request);
            return Ok(result);
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            await _registerValidator.ValidateAndThrowAsync(request);

            var insert = new UserInsertRequest
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Username = request.Username,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                Role = RoleNames.Customer,
            };

            var user = await _userService.InsertAsync(insert);

            await _notificationPublisher.PublishAsync(new NotificationMessage
            {
                UserId = user.Id,
                Title = "Dobrodošli u Svadbeni Salon Rimac",
                Body = $"Registracija uspješna, {user.FirstName}. Možete rezervisati dnevni sastanak i pregledati ponude.",
                CreatedAt = DateTime.UtcNow,
            });

            return Ok(new { message = "Registracija je uspješna. Prijavite se sa svojim korisničkim imenom." });
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
        {
            var userId = _userAccessor.GetUserId()
                         ?? throw new UnauthorizedAccessException("Niste prijavljeni.");

            await _accessManager.LogoutAsync(userId, request?.RefreshToken);
            return Ok(new { message = "Odjava uspješna. Sesija je invalidirana na serveru." });
        }

        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserPasswordChangeRequest request)
        {
            await _userService.ChangePasswordAsync(request);
            return Ok(new { message = "Lozinka je uspješno promijenjena." });
        }

        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            const string message =
                "Ako nalog postoji, poslali smo kod za reset lozinke na email.";

            var created = await _userService.RequestPasswordResetAsync(request);
            if (created != null)
            {
                await _notificationPublisher.PublishAsync(new NotificationMessage
                {
                    UserId = created.UserId,
                    RecipientEmail = created.Email,
                    Kind = "PasswordReset",
                    Title = "Reset lozinke — Svadbeni Salon Rimac",
                    Body =
                        $"Poštovani {created.FirstName},\n\n" +
                        $"Vaš kod za reset lozinke je: {created.Code}\n" +
                        "Kod važi 15 minuta.\n\n" +
                        "Ako niste tražili reset, zanemarite ovu poruku.\n\n" +
                        "Svadbeni Salon Rimac",
                    CreatedAt = DateTime.UtcNow,
                });
            }

            return Ok(new { message });
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _userService.ResetPasswordAsync(request);
            return Ok(new { message = "Lozinka je uspješno promijenjena. Prijavite se novom lozinkom." });
        }

        [HttpGet("Profile")]
        public async Task<ActionResult<UserResponse>> GetProfile()
        {
            var userId = _userAccessor.GetUserId()
                         ?? throw new UnauthorizedAccessException("Niste prijavljeni.");

            var user = await _userService.GetWithRoleByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("Profile")]
        public async Task<ActionResult<UserResponse>> UpdateProfile([FromBody] UserUpdateRequest request)
        {
            var userId = _userAccessor.GetUserId()
                         ?? throw new UnauthorizedAccessException("Niste prijavljeni.");

            var updated = await _userService.UpdateOwnProfileAsync(userId, request);
            return Ok(updated);
        }
    }
}
