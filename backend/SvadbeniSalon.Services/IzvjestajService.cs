using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services.Database;

namespace SvadbeniSalon.Services;

public interface IIzvjestajService
{
    Task<byte[]> GetSvadbePdfAsync(SvadbeIzvjestajSearchObject search);
    Task<byte[]> GetUplatePdfAsync(UplateIzvjestajSearchObject search);
}

public class IzvjestajService : IIzvjestajService
{
    private readonly SvadbeniSalonDbContext _dbContext;
    private readonly IAuthenticatedUserAccessor _userAccessor;

    public IzvjestajService(
        SvadbeniSalonDbContext dbContext,
        IAuthenticatedUserAccessor userAccessor)
    {
        _dbContext = dbContext;
        _userAccessor = userAccessor;
    }

    public async Task<byte[]> GetSvadbePdfAsync(SvadbeIzvjestajSearchObject search)
    {
        EnsureStaff();
        ValidatePeriod(search.DatumOd, search.DatumDo);

        var query = _dbContext.Svadbe
            .Include(s => s.User)
            .Include(s => s.Ponuda)
            .Where(s => s.DatumSvadbe >= search.DatumOd.Date
                        && s.DatumSvadbe <= search.DatumDo.Date);

        if (search.Status.HasValue)
        {
            query = query.Where(s => (int)s.Status == search.Status.Value);
        }

        var svadbe = await query.OrderBy(s => s.DatumSvadbe).ToListAsync();

        return IzvjestajPdfGenerator.GenerateSvadbeReport(
            search.DatumOd.Date,
            search.DatumDo.Date,
            search.Status,
            svadbe);
    }

    public async Task<byte[]> GetUplatePdfAsync(UplateIzvjestajSearchObject search)
    {
        EnsureStaff();
        ValidatePeriod(search.DatumOd, search.DatumDo);

        var uplate = await _dbContext.Rate
            .Include(r => r.Svadba)
            .ThenInclude(s => s.User)
            .Include(r => r.Svadba)
            .ThenInclude(s => s.Ponuda)
            .Where(r => r.DatumUplate >= search.DatumOd.Date
                        && r.DatumUplate <= search.DatumDo.Date)
            .OrderBy(r => r.DatumUplate)
            .ToListAsync();

        return IzvjestajPdfGenerator.GenerateUplateReport(
            search.DatumOd.Date,
            search.DatumDo.Date,
            uplate);
    }

    private void EnsureStaff()
    {
        if (!_userAccessor.IsSalonStaff())
        {
            throw new ClientException("Samo zaposlenici mogu preuzimati izvještaje.");
        }
    }

    private static void ValidatePeriod(DateTime od, DateTime doDatum)
    {
        if (doDatum.Date < od.Date)
        {
            throw new ClientException("Datum do mora biti nakon datuma od.");
        }
    }
}
