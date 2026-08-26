using FluentValidation;
using SvadbeniSalon.Model.Requests;

namespace SvadbeniSalon.Services.Validators;

public class PonudaInsertValidator : AbstractValidator<PonudaInsertRequest>
{
    public PonudaInsertValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv ponude je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Opis)
            .MaximumLength(2000).WithMessage("Opis ne smije imati više od 2000 karaktera.");

        RuleFor(x => x.Cijena)
            .GreaterThan(0).WithMessage("Cijena mora biti veća od 0 (npr. 1500.00).");

        RuleFor(x => x.MeniId)
            .GreaterThan(0).WithMessage("Odaberite meni za ponudu.");
    }
}

public class PonudaUpdateValidator : AbstractValidator<PonudaUpdateRequest>
{
    public PonudaUpdateValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv ponude je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Opis)
            .MaximumLength(2000).WithMessage("Opis ne smije imati više od 2000 karaktera.");

        RuleFor(x => x.Cijena)
            .GreaterThan(0).WithMessage("Cijena mora biti veća od 0 (npr. 1500.00).");

        RuleFor(x => x.MeniId)
            .GreaterThan(0).WithMessage("Odaberite meni za ponudu.");
    }
}
