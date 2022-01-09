using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Database.Models;
using SvadbeniSalon.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.Controllers
{
    public class DekoracijeController : BaseCRUDController<Dekoracija, Shared.Models.Dekoracija, Shared.Models.Requests.Dekoracija.AddDekoracija>
    {
        private readonly DekoracijeService _dekoracijeService;
        public DekoracijeController(DekoracijeService dekoracijeService, IMapper mapper) : base(dekoracijeService, mapper)
        {
            _dekoracijeService = dekoracijeService;
        }
    }
}
