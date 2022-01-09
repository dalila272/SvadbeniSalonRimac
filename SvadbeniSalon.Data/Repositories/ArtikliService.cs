using AutoMapper;
using SvadbeniSalon.Database.Extensions;
using SvadbeniSalon.Shared.Models;
using SvadbeniSalon.Shared.Models.Requests.Artikal;
using SvadbeniSalon.Shared.Models.Requests.Muzicar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvadbeniSalon.Database.Repositories
{
    public class ArtikliService : BaseCRUDService<Models.Artikal, Shared.Models.Artikal>
    {
        public ArtikliService(LocalContext context, IMapper mapper) : base(context, mapper)
        {

        }

        public Shared.Models.Artikal Update(Shared.Models.Artikal artikal)
        {
            if (ArtikalExists(artikal.Naziv, artikal.Id))
            {
                throw new Exception("Item with the same username already exists");
            }
            var dbArtikal = _context.Artikli.FirstOrDefault(a => a.Id == artikal.Id);
            dbArtikal.Naziv = artikal.Naziv;

            _context.SaveChanges();

            return artikal;
        }


        public PagedResult<Shared.Models.Artikal> Filter(PagedRequestBase<FilterArtikal> pagedQuery)
        {
            var query = _context.Artikli.AsQueryable();

            if (!string.IsNullOrEmpty(pagedQuery?.Query?.Naziv))
            {
                query = query.Where(a => a.Naziv.Contains(pagedQuery.Query.Naziv));
            }
            if (pagedQuery.Query.Tip != null)
            {
                query = query.Where(a => a.Tip == pagedQuery.Query.Tip);
            }

            return query.GetPaginatedResult<Models.Artikal, Shared.Models.Artikal, FilterArtikal>(_mapper, pagedQuery);
        }

        public bool ArtikalExists(string name, long id)
        {
            return _context.Artikli.Where(x => x.DeletedAt == null && x.Naziv == name && x.Id != id).Any();
        }
    }
}
