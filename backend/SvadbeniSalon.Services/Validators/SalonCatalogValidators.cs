using FluentValidation;
using SvadbeniSalon.Model.Requests;

namespace SvadbeniSalon.Services.Validators;

public class MuzicarInsertValidator : AbstractValidator<MuzicarInsertRequest>
{
    public MuzicarInsertValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv muzičara je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Opis)
            .MaximumLength(2000).WithMessage("Opis ne smije imati više od 2000 karaktera.");
    }
}

public class MuzicarUpdateValidator : AbstractValidator<MuzicarUpdateRequest>
{
    public MuzicarUpdateValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv muzičara je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Opis)
            .MaximumLength(2000).WithMessage("Opis ne smije imati više od 2000 karaktera.");
    }
}

public class DekoracijaInsertValidator : AbstractValidator<DekoracijaInsertRequest>
{
    public DekoracijaInsertValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv dekoracije je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Opis)
            .MaximumLength(2000).WithMessage("Opis ne smije imati više od 2000 karaktera.");

        RuleFor(x => x.Cijena)
            .GreaterThanOrEqualTo(0).WithMessage("Cijena ne smije biti negativna (npr. 150.00).");
    }
}

public class DekoracijaUpdateValidator : AbstractValidator<DekoracijaUpdateRequest>
{
    public DekoracijaUpdateValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv dekoracije je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Opis)
            .MaximumLength(2000).WithMessage("Opis ne smije imati više od 2000 karaktera.");

        RuleFor(x => x.Cijena)
            .GreaterThanOrEqualTo(0).WithMessage("Cijena ne smije biti negativna (npr. 150.00).");
    }
}

public class ZanrInsertValidator : AbstractValidator<ZanrInsertRequest>
{
    public ZanrInsertValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv žanra je obavezan.")
            .MaximumLength(100).WithMessage("Naziv ne smije imati više od 100 karaktera.");
    }
}

public class ZanrUpdateValidator : AbstractValidator<ZanrUpdateRequest>
{
    public ZanrUpdateValidator()
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv žanra je obavezan.")
            .MaximumLength(100).WithMessage("Naziv ne smije imati više od 100 karaktera.");
    }
}
