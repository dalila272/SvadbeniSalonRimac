using SvadbeniSalon.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;

namespace SvadbeniSalon.Database.Repositories
{
    public class DekoracijeService : BaseCRUDService<Dekoracija, Shared.Models.Dekoracija>
    {
        public DekoracijeService(LocalContext context, IMapper mapper) : base(context, mapper)
        {

        }
    }
}
