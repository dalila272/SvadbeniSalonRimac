using FluentValidation;
using SvadbeniSalon.Model.Requests;

namespace SvadbeniSalon.Services.Validators;

public class MeniInsertValidator : AbstractValidator<MeniInsertRequest>
{
    public MeniInsertValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv menija je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Opis)
            .MaximumLength(2000).WithMessage("Opis ne smije imati više od 2000 karaktera.");

        RuleFor(x => x.Cijena)
            .GreaterThanOrEqualTo(0).WithMessage("Cijena ne smije biti negativna (npr. 50.00).");

        RuleFor(x => x)
            .Must(x => x.HranaIds.Count > 0 || x.PiceIds.Count > 0)
            .WithMessage("Odaberite barem jednu hranu ili piće.");
    }
}

public class MeniUpdateValidator : AbstractValidator<MeniUpdateRequest>
{
    public MeniUpdateValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv menija je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Opis)
            .MaximumLength(2000).WithMessage("Opis ne smije imati više od 2000 karaktera.");

        RuleFor(x => x.Cijena)
            .GreaterThanOrEqualTo(0).WithMessage("Cijena ne smije biti negativna (npr. 50.00).");

        RuleFor(x => x)
            .Must(x => x.HranaIds.Count > 0 || x.PiceIds.Count > 0)
            .WithMessage("Odaberite barem jednu hranu ili piće.");
    }
}
