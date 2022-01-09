using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SvadbeniSalon.Database.Extensions;
using SvadbeniSalon.Shared.Models;
using SvadbeniSalon.Shared.Models.Requests.Muzicar;
using SvadbeniSalon.Shared.Models.Requests.Ponuda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvadbeniSalon.Database.Repositories
{
    public class PonudeService : BaseCRUDService<Models.Ponuda, Shared.Models.Ponuda>
    {
        public PonudeService(LocalContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public PagedResult<Shared.Models.Ponuda> Filter(PagedRequestBase<FilterPonuda> pagedQuery)
        {
            var query = _context.Ponude.AsQueryable();



            if (pagedQuery.Query?.MeniId != null)
            {
                query = query.Where(p => p.MeniId == pagedQuery.Query.MeniId);
            }

            if (pagedQuery?.Query?.DekoracijeIds != null)
            {
                query = query.Where(p => p.DekoracijePonuda.Any(dp => pagedQuery.Query.DekoracijeIds.Contains(dp.DekoracijaId)));
            }


            if (!string.IsNullOrEmpty(pagedQuery.Query?.Muzicar))
            {
                query = query.Where(p => p.MuzicariPonuda.Any(mp => mp.Muzicar.Naziv.Contains(pagedQuery.Query.Muzicar)));
            }

            query = query.Include(p => p.DekoracijePonuda).ThenInclude(dp => dp.Ponuda)
                         .Include(p => p.MuzicariPonuda).ThenInclude(mp => mp.Muzicar);

            return query.GetPaginatedResult<Models.Ponuda, Shared.Models.Ponuda, FilterPonuda>(_mapper, pagedQuery);
        }
    }
}
