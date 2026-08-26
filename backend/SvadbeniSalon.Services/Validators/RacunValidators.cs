using FluentValidation;
using SvadbeniSalon.Model.Requests;

namespace SvadbeniSalon.Services.Validators;

public class RacunInsertValidator : AbstractValidator<RacunInsertRequest>
{
    public RacunInsertValidator()
    {
        RuleFor(x => x.SvadbaId)
            .GreaterThan(0).WithMessage("Svadba je obavezna.");
    }
}
