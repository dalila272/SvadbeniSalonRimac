using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Services;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Model.Responses;
using Microsoft.AspNetCore.Authorization;

namespace SvadbeniSalon.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public abstract class BaseReadController<TResponse, TSearch, TService> : ControllerBase
    where TSearch : BaseSearchObject
    where TService : IBaseReadService<TResponse, TSearch>
{
    protected readonly TService _service;

    protected BaseReadController(TService service)
    {
        _service = service;
    }

    [HttpGet]
    public virtual async Task<PageResult<TResponse>> GetAll([FromQuery] TSearch? search)
    {
        var results = await _service.GetAllAsync(search);
        return results;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TResponse>> GetById(int id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
