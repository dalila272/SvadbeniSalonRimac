using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Model.Constants;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.WebAPI.Controllers;

[Authorize]
public class PonudeController
    : BaseCRUDController<PonudaResponse, PonudaSearchObject, PonudaInsertRequest, PonudaUpdateRequest, IPonudaService>
{
    public PonudeController(IPonudaService service) : base(service)
    {
    }

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<PonudaResponse>> Create([FromBody] PonudaInsertRequest request)
        => base.Create(request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<PonudaResponse>> Update(int id, [FromBody] PonudaUpdateRequest request)
        => base.Update(id, request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<IActionResult> Delete(int id)
        => base.Delete(id);
}
