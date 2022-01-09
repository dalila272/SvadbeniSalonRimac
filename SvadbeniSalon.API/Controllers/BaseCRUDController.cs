using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.API.Services.Interfaces;
using SvadbeniSalon.Database.Repositories;
using SvadbeniSalon.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvadbeniSalon.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseCRUDController<TEntity, TModel, TInsert> : BaseController<TEntity, TModel> where TEntity : class
    {

        private readonly BaseCRUDService<TEntity, TModel> _crudService;
        public BaseCRUDController(BaseCRUDService<TEntity, TModel> service, IMapper mapper) : base(service, mapper)
        {
            _crudService = service;
        }


        [HttpPost]
        public virtual ActionResult<TModel> Insert(TInsert request)
        {
            return _crudService.Add(_mapper.Map<TModel>(request));
        }


        [HttpDelete]
        public IActionResult Remove(long id)
        {
            _crudService.Remove(id);
            return Ok();
        }

    }
}
