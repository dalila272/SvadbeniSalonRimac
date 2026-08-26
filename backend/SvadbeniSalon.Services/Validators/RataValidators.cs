using FluentValidation;
using SvadbeniSalon.Model.Requests;

namespace SvadbeniSalon.Services.Validators;

public class RataInsertValidator : AbstractValidator<RataInsertRequest>
{
    public RataInsertValidator()
    {
        RuleFor(x => x.SvadbaId)
            .GreaterThan(0).WithMessage("Svadba je obavezna.");

        RuleFor(x => x.Iznos)
            .GreaterThan(0).WithMessage("Iznos uplate mora biti veći od 0.");

        RuleFor(x => x.DatumUplate)
            .Must(d => d.Date <= DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("Datum uplate nije validan.");
    }
}
