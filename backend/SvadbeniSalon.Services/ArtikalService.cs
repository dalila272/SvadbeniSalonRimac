using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Model.Exceptions;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services.Database;
using FluentValidation;

namespace SvadbeniSalon.Services;

public interface IArtikalService
    : IBaseCRUDService<ArtikalResponse, ArtikalSearchObject, ArtikalInsertRequest, ArtikalUpdateRequest> { }

public class ArtikalService
    : BaseCRUDService<Artikal, ArtikalResponse, ArtikalSearchObject, ArtikalInsertRequest, ArtikalUpdateRequest>,
        IArtikalService
{
    public ArtikalService(
        SvadbeniSalonDbContext dbContext,
        MapsterMapper.IMapper mapper,
        IValidator<ArtikalInsertRequest> insertValidator,
        IValidator<ArtikalUpdateRequest> updateValidator)
        : base(dbContext, mapper, insertValidator, updateValidator)
    {
    }

    protected override IQueryable<Artikal> ApplyFilters(IQueryable<Artikal> query, ArtikalSearchObject? search)
    {
        if (!string.IsNullOrWhiteSpace(search?.Naziv))
        {
            var term = search.Naziv.Trim().ToLower();
            query = query.Where(a => a.Naziv.ToLower().Contains(term));
        }

        if (search?.Tip.HasValue == true)
        {
            query = query.Where(a => a.Tip == search.Tip.Value);
        }

        if (search?.IsActive.HasValue == true)
        {
            query = query.Where(a => a.IsActive == search.IsActive.Value);
        }

        return query;
    }

    public override async Task DeleteAsync(int id)
    {
        var inUse = await _dbContext.Set<MeniArtikal>().AnyAsync(ma => ma.ArtikalId == id);
        if (inUse)
        {
            throw new ClientException("Artikal se koristi u menijima i ne može se obrisati.");
        }

        await base.DeleteAsync(id);
    }
}
