using SvadbeniSalon.Model.Requests;
using FluentValidation;

namespace SvadbeniSalon.Services.Validators
{
    public class UserUpdateValidator : AbstractValidator<UserUpdateRequest>
    {
        public UserUpdateValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ime je obavezno.")
                .MaximumLength(50).WithMessage("Ime ne smije imati više od 50 karaktera.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Prezime je obavezno.")
                .MaximumLength(50).WithMessage("Prezime ne smije imati više od 50 karaktera.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email je obavezan.")
                .EmailAddress().WithMessage("Unesite ispravan email u formatu: ime@domena.com")
                .MaximumLength(100).WithMessage("Email ne smije imati više od 100 karaktera.");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Korisničko ime je obavezno.")
                .MinimumLength(3).WithMessage("Korisničko ime mora imati najmanje 3 karaktera.")
                .MaximumLength(100).WithMessage("Korisničko ime ne smije imati više od 100 karaktera.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Telefon ne smije imati više od 20 karaktera.")
                .Matches(@"^[+0-9\s\-()/]+$")
                .WithMessage("Telefon smije sadržavati samo brojeve, razmake, +, -, ( ).")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}
