using FluentValidation;
using SvadbeniSalon.Model.Requests;

namespace SvadbeniSalon.Services.Validators;

public class RecenzijaInsertValidator : AbstractValidator<RecenzijaInsertRequest>
{
    public RecenzijaInsertValidator()
    {
        RuleFor(x => x.PonudaId)
            .GreaterThan(0).WithMessage("Paket je obavezan.");

        RuleFor(x => x.SvadbaId)
            .NotNull().WithMessage("Svadba je obavezna.")
            .GreaterThan(0).WithMessage("Svadba nije validna.");

        RuleFor(x => x.Ocjena)
            .InclusiveBetween(1, 5).WithMessage("Ocjena mora biti između 1 i 5.");

        RuleFor(x => x.Komentar)
            .MaximumLength(1000);
    }
}

public class RecenzijaUpdateValidator : AbstractValidator<RecenzijaUpdateRequest>
{
    public RecenzijaUpdateValidator()
    {
        RuleFor(x => x.Ocjena)
            .InclusiveBetween(1, 5).WithMessage("Ocjena mora biti između 1 i 5.");

        RuleFor(x => x.Komentar)
            .MaximumLength(1000);
    }
}
