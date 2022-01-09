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
    [Route("api/[controller]")]
    [ApiController]
    public class ZanroviController : BaseController<Zanr, Shared.Models.Zanr>
    {
        private readonly ZanroviService _zanroviService;
        public ZanroviController(ZanroviService zanroviService, IMapper mapper) : base(zanroviService, mapper)
        {
            _zanroviService = zanroviService;
        }
    }
}
