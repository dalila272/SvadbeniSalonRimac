using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Enums;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Services.Database;

namespace SvadbeniSalon.Services.Validators;

public class MeniInsertValidator : AbstractValidator<MeniInsertRequest>
{
    public MeniInsertValidator(SvadbeniSalonDbContext db)
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

        RuleFor(x => x.HranaIds)
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("Lista hrane sadrži neispravan ID.")
            .MustAsync(async (ids, ct) =>
                await AllArtikliExistAsync(db, ids, TipArtikla.Hrana, ct))
            .WithMessage("Jedna ili više odabranih stavki hrane ne postoje, nisu aktivne ili nisu tipa hrana.");

        RuleFor(x => x.PiceIds)
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("Lista pića sadrži neispravan ID.")
            .MustAsync(async (ids, ct) =>
                await AllArtikliExistAsync(db, ids, TipArtikla.Pice, ct))
            .WithMessage("Jedno ili više odabranih pića ne postoje, nisu aktivna ili nisu tipa piće.");
    }

    internal static async Task<bool> AllArtikliExistAsync(
        SvadbeniSalonDbContext db,
        IEnumerable<int>? ids,
        TipArtikla tip,
        CancellationToken ct)
    {
        var list = ids?.Where(id => id > 0).Distinct().ToList() ?? [];
        if (list.Count == 0) return true;

        var found = await db.Artikli.CountAsync(
            a => list.Contains(a.Id) && a.Tip == tip && a.IsActive,
            ct);
        return found == list.Count;
    }
}

public class MeniUpdateValidator : AbstractValidator<MeniUpdateRequest>
{
    public MeniUpdateValidator(SvadbeniSalonDbContext db)
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

        RuleFor(x => x.HranaIds)
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("Lista hrane sadrži neispravan ID.")
            .MustAsync(async (ids, ct) =>
                await MeniInsertValidator.AllArtikliExistAsync(db, ids, TipArtikla.Hrana, ct))
            .WithMessage("Jedna ili više odabranih stavki hrane ne postoje, nisu aktivne ili nisu tipa hrana.");

        RuleFor(x => x.PiceIds)
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("Lista pića sadrži neispravan ID.")
            .MustAsync(async (ids, ct) =>
                await MeniInsertValidator.AllArtikliExistAsync(db, ids, TipArtikla.Pice, ct))
            .WithMessage("Jedno ili više odabranih pića ne postoje, nisu aktivna ili nisu tipa piće.");
    }
}
