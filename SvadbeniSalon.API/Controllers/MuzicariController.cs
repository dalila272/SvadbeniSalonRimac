using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.API.DTO.Requests.Muzicar;
using SvadbeniSalon.Database.Models;
using SvadbeniSalon.Database.Repositories;
using SvadbeniSalon.Shared.Models;
using SvadbeniSalon.Shared.Models.Requests.Muzicar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.Controllers
{
    public class MuzicariController : BaseCRUDController<Database.Models.Muzicar, Shared.Models.Muzicar, AddMuzicar>
    {
        private readonly MuzicariService _muzicariService;
        public MuzicariController(MuzicariService muzicariService, IMapper mapper) : base(muzicariService, mapper)
        {
            _muzicariService = muzicariService;
        }

        [HttpGet("filter")]
        public ActionResult<PagedResult<Shared.Models.Muzicar>> Filter([FromQuery] PagedRequestBase<FilterMuzicar> pagedQuery)
        {
            return Ok(_muzicariService.Filter(pagedQuery));
        }

    }
}
