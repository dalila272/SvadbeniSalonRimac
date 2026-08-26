using FluentValidation;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.Services.Validators;

public class DnevniSastanakInsertValidator : AbstractValidator<DnevniSastanakInsertRequest>
{
    public DnevniSastanakInsertValidator()
    {
        RuleFor(x => x.DatumSastanka)
            .Must(d => d > DateTime.UtcNow)
            .WithMessage("Datum sastanka mora biti u budućnosti.");

        RuleFor(x => x.DatumSastanka)
            .Must(DnevniSastanakSlotRules.IsValidStartTime)
            .WithMessage("Termin mora biti na puni sat ili pola sata (npr. 09:00, 09:30).");

        RuleFor(x => x.Napomena)
            .MaximumLength(1000);

        RuleFor(x => x.KontaktIme)
            .MaximumLength(200);
    }
}

public class DnevniSastanakUpdateValidator : AbstractValidator<DnevniSastanakUpdateRequest>
{
    public DnevniSastanakUpdateValidator()
    {
        RuleFor(x => x.DatumSastanka)
            .Must(d => d > DateTime.UtcNow)
            .WithMessage("Datum sastanka mora biti u budućnosti.");

        RuleFor(x => x.DatumSastanka)
            .Must(DnevniSastanakSlotRules.IsValidStartTime)
            .WithMessage("Termin mora biti na puni sat ili pola sata (npr. 09:00, 09:30).");

        RuleFor(x => x.Napomena)
            .MaximumLength(1000);
    }
}
