using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.API.DTO.Requests.Meni;
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
    public class MeniController : BaseCRUDController<Database.Models.Meni, Shared.Models.Meni, AddMeni>
    {
        private readonly MeniService _meniService;
        public MeniController(MeniService meniService, IMapper mapper) : base(meniService, mapper)
        {
            _meniService = meniService;
        }
    }
}
