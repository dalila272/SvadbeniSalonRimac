using AutoMapper;
using SvadbeniSalon.Database.Extensions;
using SvadbeniSalon.Database.Models;
using SvadbeniSalon.Shared.Models;
using SvadbeniSalon.Shared.Models.Requests.Muzicar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SvadbeniSalon.Database.Repositories
{
    public class MuzicariService : BaseCRUDService<Models.Muzicar, Shared.Models.Muzicar>
    {


        public MuzicariService(LocalContext context, IMapper mapper) : base(context, mapper)
        {


        }

        public PagedResult<Shared.Models.Muzicar> Filter(PagedRequestBase<FilterMuzicar> pagedQuery)
        {
            var query = _context.Muzicari.AsQueryable();

            if (!string.IsNullOrEmpty(pagedQuery?.Query?.Naziv))
            {
                query = query.Where(m => m.Naziv.Contains(pagedQuery.Query.Naziv));
            }

            if (pagedQuery?.Query?.Zanrovi != null)
            {
                query = query.Where(m => m.Zanrovi.Any(x => pagedQuery.Query.Zanrovi.Contains(x.ZanrId)));
            }

            return query.GetPaginatedResult<Models.Muzicar, Shared.Models.Muzicar, FilterMuzicar>(_mapper, pagedQuery);
        }

    }
}
