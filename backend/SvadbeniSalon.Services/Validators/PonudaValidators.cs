using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Services.Database;

namespace SvadbeniSalon.Services.Validators;

public class PonudaInsertValidator : AbstractValidator<PonudaInsertRequest>
{
    public PonudaInsertValidator(SvadbeniSalonDbContext db)
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv ponude je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Opis)
            .MaximumLength(2000).WithMessage("Opis ne smije imati više od 2000 karaktera.");

        RuleFor(x => x.Cijena)
            .GreaterThan(0).WithMessage("Cijena mora biti veća od 0 (npr. 1500.00).");

        RuleFor(x => x.MeniId)
            .GreaterThan(0).WithMessage("Odaberite meni za ponudu.")
            .MustAsync(async (meniId, ct) =>
                await db.Meniji.AnyAsync(m => m.Id == meniId && m.IsActive, ct))
            .WithMessage("Odabrani meni ne postoji ili nije aktivan.");

        RuleFor(x => x.MuzicarIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("Lista muzičara sadrži neispravan ID.")
            .MustAsync(async (ids, ct) => await AllMuzicariExistAsync(db, ids, ct))
            .WithMessage("Jedan ili više odabranih muzičara ne postoje ili nisu aktivni.");

        RuleFor(x => x.DekoracijaIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("Lista dekoracija sadrži neispravan ID.")
            .MustAsync(async (ids, ct) => await AllDekoracijeExistAsync(db, ids, ct))
            .WithMessage("Jedna ili više odabranih dekoracija ne postoje ili nisu aktivne.");
    }

    internal static async Task<bool> AllMuzicariExistAsync(
        SvadbeniSalonDbContext db,
        IEnumerable<int>? ids,
        CancellationToken ct)
    {
        var list = ids?.Where(id => id > 0).Distinct().ToList() ?? [];
        if (list.Count == 0) return true;
        var found = await db.Muzicari.CountAsync(m => list.Contains(m.Id) && m.IsActive, ct);
        return found == list.Count;
    }

    internal static async Task<bool> AllDekoracijeExistAsync(
        SvadbeniSalonDbContext db,
        IEnumerable<int>? ids,
        CancellationToken ct)
    {
        var list = ids?.Where(id => id > 0).Distinct().ToList() ?? [];
        if (list.Count == 0) return true;
        var found = await db.Dekoracije.CountAsync(d => list.Contains(d.Id) && d.IsActive, ct);
        return found == list.Count;
    }
}

public class PonudaUpdateValidator : AbstractValidator<PonudaUpdateRequest>
{
    public PonudaUpdateValidator(SvadbeniSalonDbContext db)
    {
        RuleFor(x => x.Naziv)
            .NotEmpty().WithMessage("Naziv ponude je obavezan.")
            .MaximumLength(200).WithMessage("Naziv ne smije imati više od 200 karaktera.");

        RuleFor(x => x.Opis)
            .MaximumLength(2000).WithMessage("Opis ne smije imati više od 2000 karaktera.");

        RuleFor(x => x.Cijena)
            .GreaterThan(0).WithMessage("Cijena mora biti veća od 0 (npr. 1500.00).");

        RuleFor(x => x.MeniId)
            .GreaterThan(0).WithMessage("Odaberite meni za ponudu.")
            .MustAsync(async (meniId, ct) =>
                await db.Meniji.AnyAsync(m => m.Id == meniId && m.IsActive, ct))
            .WithMessage("Odabrani meni ne postoji ili nije aktivan.");

        RuleFor(x => x.MuzicarIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("Lista muzičara sadrži neispravan ID.")
            .MustAsync(async (ids, ct) =>
                await PonudaInsertValidator.AllMuzicariExistAsync(db, ids, ct))
            .WithMessage("Jedan ili više odabranih muzičara ne postoje ili nisu aktivni.");

        RuleFor(x => x.DekoracijaIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("Lista dekoracija sadrži neispravan ID.")
            .MustAsync(async (ids, ct) =>
                await PonudaInsertValidator.AllDekoracijeExistAsync(db, ids, ct))
            .WithMessage("Jedna ili više odabranih dekoracija ne postoje ili nisu aktivne.");
    }
}
