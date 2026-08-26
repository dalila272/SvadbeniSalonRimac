using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.WebAPI.Controllers;

[Authorize]
public class DnevniSastanciController
    : BaseCRUDController<DnevniSastanakResponse, DnevniSastanakSearchObject, DnevniSastanakInsertRequest, DnevniSastanakUpdateRequest, IDnevniSastanakService>
{
    public DnevniSastanciController(IDnevniSastanakService service) : base(service)
    {
    }

    [HttpGet("zauzeti")]
    public async Task<ActionResult<List<DateTime>>> GetZauzeti()
    {
        var result = await ((IDnevniSastanakService)_service).GetZauzetiDatumiAsync();
        return result;
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<DnevniSastanakResponse>> ChangeStatus(int id, [FromBody] TerminStatusChangeRequest request)
    {
        var result = await ((IDnevniSastanakService)_service).ChangeStatusAsync(id, request);
        return result;
    }
}
