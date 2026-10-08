using FluentValidation;
using SvadbeniSalon.Model.Requests;

namespace SvadbeniSalon.Services.Validators;

public class SvadbaInsertValidator : AbstractValidator<SvadbaInsertRequest>
{
    public SvadbaInsertValidator()
    {
        RuleFor(x => x.PonudaId)
            .GreaterThan(0).WithMessage("Paket je obavezan.");

        RuleFor(x => x.DatumSvadbe)
            .Must(d => d.Date >= SalonClock.MinWeddingBookingDate)
            .WithMessage(
                $"Datum svadbe mora biti najmanje {SalonClock.MinWeddingLeadDays} dana unaprijed.");

        RuleFor(x => x.Vrijeme)
            .Must(v => v >= TimeSpan.FromHours(14) && v <= TimeSpan.FromHours(21))
            .WithMessage("Vrijeme mora biti između 14:00 i 21:00.");

        RuleFor(x => x)
            .Must(x => SalonClock.IsInFuture(x.DatumSvadbe, x.Vrijeme))
            .WithMessage("Termin svadbe (datum i vrijeme) ne smije biti u prošlosti.");

        RuleFor(x => x.BrojGostiju)
            .GreaterThan(0).WithMessage("Broj gostiju mora biti veći od 0.");

        RuleFor(x => x.BrojRata)
            .InclusiveBetween(1, 12).WithMessage("Broj rata mora biti između 1 i 12.");

        RuleFor(x => x.Napomena)
            .MaximumLength(1000);
    }
}

public class SvadbaUpdateValidator : AbstractValidator<SvadbaUpdateRequest>
{
    public SvadbaUpdateValidator()
    {
        RuleFor(x => x.PonudaId)
            .GreaterThan(0).WithMessage("Paket je obavezan.");

        // Lead-time se na update-u provjerava u servisu samo ako se datum mijenja.
        RuleFor(x => x.DatumSvadbe)
            .Must(d => d.Date >= SalonClock.Today)
            .WithMessage("Datum svadbe mora biti danas ili u budućnosti.");

        RuleFor(x => x.Vrijeme)
            .Must(v => v >= TimeSpan.FromHours(14) && v <= TimeSpan.FromHours(21))
            .WithMessage("Vrijeme mora biti između 14:00 i 21:00.");

        RuleFor(x => x)
            .Must(x => SalonClock.IsInFuture(x.DatumSvadbe, x.Vrijeme))
            .WithMessage("Termin svadbe (datum i vrijeme) ne smije biti u prošlosti.");

        RuleFor(x => x.BrojGostiju)
            .GreaterThan(0).WithMessage("Broj gostiju mora biti veći od 0.");

        RuleFor(x => x.BrojRata)
            .InclusiveBetween(1, 12).WithMessage("Broj rata mora biti između 1 i 12.");

        RuleFor(x => x.Napomena)
            .MaximumLength(1000);
    }
}
