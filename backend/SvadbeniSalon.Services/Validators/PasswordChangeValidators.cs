using FluentValidation;
using SvadbeniSalon.Model.Requests;

namespace SvadbeniSalon.Services.Validators;

public class UserPasswordChangeValidator : AbstractValidator<UserPasswordChangeRequest>
{
    public UserPasswordChangeValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Trenutna lozinka je obavezna.");
        RuleFor(x => x.NewPassword).ApplyPasswordRules();
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Potvrda lozinke se ne poklapa s novom lozinkom.");
    }
}

public class AdminSetPasswordValidator : AbstractValidator<AdminSetPasswordRequest>
{
    public AdminSetPasswordValidator()
    {
        RuleFor(x => x.NewPassword).ApplyPasswordRules();
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("Potvrda lozinke se ne poklapa s novom lozinkom.");
    }
}
