using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.WebAPI.Controllers;

[Authorize]
public class SvadbeController
    : BaseCRUDController<SvadbaResponse, SvadbaSearchObject, SvadbaInsertRequest, SvadbaUpdateRequest, ISvadbaService>
{
    public SvadbeController(ISvadbaService service) : base(service)
    {
    }

    [HttpGet("zauzeti")]
    public async Task<ActionResult<List<DateTime>>> GetZauzeti()
    {
        var result = await ((ISvadbaService)_service).GetZauzetiDatumiAsync();
        return result;
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<SvadbaResponse>> ChangeStatus(int id, [FromBody] TerminStatusChangeRequest request)
    {
        var result = await ((ISvadbaService)_service).ChangeStatusAsync(id, request);
        return result;
    }
}
