using FluentValidation;

namespace SvadbeniSalon.Services.Validators;

public static class PasswordRules
{
    public const int MinLength = 8;
    public const int MaxLength = 100;

    public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(
        this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Lozinka je obavezna.")
            .MinimumLength(MinLength)
            .WithMessage($"Lozinka mora imati najmanje {MinLength} karaktera.")
            .MaximumLength(MaxLength)
            .WithMessage($"Lozinka ne smije imati više od {MaxLength} karaktera.")
            .Matches(@"[A-Za-z]").WithMessage("Lozinka mora sadržavati barem jedno slovo.")
            .Matches(@"[0-9]").WithMessage("Lozinka mora sadržavati barem jedan broj.");
    }

    public static void EnsurePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new Model.Exceptions.ClientException("Lozinka je obavezna.");
        if (password.Length < MinLength)
            throw new Model.Exceptions.ClientException(
                $"Lozinka mora imati najmanje {MinLength} karaktera.");
        if (password.Length > MaxLength)
            throw new Model.Exceptions.ClientException(
                $"Lozinka ne smije imati više od {MaxLength} karaktera.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Za-z]"))
            throw new Model.Exceptions.ClientException(
                "Lozinka mora sadržavati barem jedno slovo.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]"))
            throw new Model.Exceptions.ClientException(
                "Lozinka mora sadržavati barem jedan broj.");
    }
}
