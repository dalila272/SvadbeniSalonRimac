using AutoMapper;
using SvadbeniSalon.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvadbeniSalon.Database.Repositories
{
    public class ZanroviService : BaseService<Zanr, Shared.Models.Zanr>
    {
        public ZanroviService(LocalContext context, IMapper mapper) : base(context, mapper)
        {

        }
    }
}
