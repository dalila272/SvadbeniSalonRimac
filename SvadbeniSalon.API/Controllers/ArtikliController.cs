using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.API.DTO.Requests.Muzicar;
using SvadbeniSalon.Database.Models;
using SvadbeniSalon.Database.Repositories;
using SvadbeniSalon.Shared.Models;
using SvadbeniSalon.Shared.Models.Requests.Artikal;
using SvadbeniSalon.Shared.Models.Requests.Muzicar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.Controllers
{
    public class ArtikliController : BaseCRUDController<Database.Models.Artikal, Shared.Models.Artikal, AddArtikal>
    {
        private readonly ArtikliService _artikliService;
        public ArtikliController(ArtikliService artikliService, IMapper mapper) : base(artikliService, mapper)
        {
            _artikliService = artikliService;

        }

        [HttpGet("filter")]
        public ActionResult<PagedResult<Shared.Models.Artikal>> Filter([FromQuery] PagedRequestBase<FilterArtikal> pagedQuery)
        {
            return Ok(_artikliService.Filter(pagedQuery));
        }

    }
}
