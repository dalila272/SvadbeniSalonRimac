using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.API.DTO.Requests.Ponuda;
using SvadbeniSalon.Database.Repositories;
using SvadbeniSalon.Shared.Models;
using SvadbeniSalon.Shared.Models.Requests.Ponuda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.Controllers
{
    public class PonudeController : BaseCRUDController<Database.Models.Ponuda, Shared.Models.Ponuda, AddPonuda>
    {
        private readonly PonudeService _ponudeService;
        public PonudeController(PonudeService ponudeService, IMapper mapper) : base(ponudeService, mapper)
        {
            _ponudeService = ponudeService;
        }

        [HttpGet("filter")]
        public ActionResult<PagedResult<Shared.Models.Ponuda>> Filter([FromQuery] PagedRequestBase<FilterPonuda> pagedQuery)
        {
            return Ok(_ponudeService.Filter(pagedQuery));
        }

    }
}