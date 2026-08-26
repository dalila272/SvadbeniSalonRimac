using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Model.Constants;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.WebAPI.Controllers;

[Authorize(Roles = RoleNames.AdminOrZaposlenik)]
[ApiController]
[Route("[controller]")]
public class RateController : ControllerBase
{
    private readonly IRataService _service;

    public RateController(IRataService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<PageResult<RataResponse>> GetAll([FromQuery] RataSearchObject? search)
    {
        return await _service.GetAllAsync(search);
    }

    [HttpPost]
    public async Task<ActionResult<RataResponse>> Create([FromBody] RataInsertRequest request)
    {
        var result = await _service.InsertAsync(request);
        return result;
    }
}
