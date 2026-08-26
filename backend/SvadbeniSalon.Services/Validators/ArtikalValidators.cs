using FluentValidation;
using SvadbeniSalon.Model.Requests;

namespace SvadbeniSalon.Services.Validators;

public class ArtikalInsertValidator : AbstractValidator<ArtikalInsertRequest>
{
    public ArtikalInsertValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv artikla je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Tip)
            .IsInEnum().WithMessage("Tip artikla nije validan (Hrana ili Piće).");

        RuleFor(x => x.Cijena)
            .GreaterThan(0).WithMessage("Cijena mora biti veća od 0 (npr. 15.00).");
    }
}

public class ArtikalUpdateValidator : AbstractValidator<ArtikalUpdateRequest>
{
    public ArtikalUpdateValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv artikla je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Tip)
            .IsInEnum().WithMessage("Tip artikla nije validan (Hrana ili Piće).");

        RuleFor(x => x.Cijena)
            .GreaterThan(0).WithMessage("Cijena mora biti veća od 0 (npr. 15.00).");
    }
}
