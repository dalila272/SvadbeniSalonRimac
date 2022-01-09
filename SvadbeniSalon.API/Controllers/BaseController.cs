using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.Controllers
{
    public class BaseController<TEntity, TModel> : ControllerBase where TEntity : class
    {
        protected readonly BaseService<TEntity, TModel> _service;
        protected readonly IMapper _mapper;
        public BaseController(BaseService<TEntity, TModel> service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public virtual IList<TModel> GetAll()
        {
            return _service.GetAll();
        }
    }
}
