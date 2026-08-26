using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Model.Constants;
using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.WebAPI.Controllers;

[Authorize]
public class MenijiController
    : BaseCRUDController<MeniResponse, MeniSearchObject, MeniInsertRequest, MeniUpdateRequest, IMeniService>
{
    public MenijiController(IMeniService service) : base(service) { }

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<MeniResponse>> Create([FromBody] MeniInsertRequest request)
        => base.Create(request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<MeniResponse>> Update(int id, [FromBody] MeniUpdateRequest request)
        => base.Update(id, request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<IActionResult> Delete(int id)
        => base.Delete(id);
}

[Authorize]
public class ArtikliController
    : BaseCRUDController<ArtikalResponse, ArtikalSearchObject, ArtikalInsertRequest, ArtikalUpdateRequest, IArtikalService>
{
    public ArtikliController(IArtikalService service) : base(service) { }

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<ArtikalResponse>> Create([FromBody] ArtikalInsertRequest request)
        => base.Create(request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<ArtikalResponse>> Update(int id, [FromBody] ArtikalUpdateRequest request)
        => base.Update(id, request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<IActionResult> Delete(int id)
        => base.Delete(id);
}

[Authorize]
public class MuzicariController
    : BaseCRUDController<MuzicarResponse, MuzicarSearchObject, MuzicarInsertRequest, MuzicarUpdateRequest, IMuzicarService>
{
    public MuzicariController(IMuzicarService service) : base(service) { }

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<MuzicarResponse>> Create([FromBody] MuzicarInsertRequest request)
        => base.Create(request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<MuzicarResponse>> Update(int id, [FromBody] MuzicarUpdateRequest request)
        => base.Update(id, request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<IActionResult> Delete(int id)
        => base.Delete(id);
}

[Authorize]
public class DekoracijeController
    : BaseCRUDController<DekoracijaResponse, DekoracijaSearchObject, DekoracijaInsertRequest, DekoracijaUpdateRequest, IDekoracijaService>
{
    public DekoracijeController(IDekoracijaService service) : base(service) { }

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<DekoracijaResponse>> Create([FromBody] DekoracijaInsertRequest request)
        => base.Create(request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<DekoracijaResponse>> Update(int id, [FromBody] DekoracijaUpdateRequest request)
        => base.Update(id, request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<IActionResult> Delete(int id)
        => base.Delete(id);
}

[Authorize]
public class ZanroviController
    : BaseCRUDController<ZanrResponse, ZanrSearchObject, ZanrInsertRequest, ZanrUpdateRequest, IZanrService>
{
    public ZanroviController(IZanrService service) : base(service) { }

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<ZanrResponse>> Create([FromBody] ZanrInsertRequest request)
        => base.Create(request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<ActionResult<ZanrResponse>> Update(int id, [FromBody] ZanrUpdateRequest request)
        => base.Update(id, request);

    [Authorize(Roles = RoleNames.AdminOrZaposlenik)]
    public override Task<IActionResult> Delete(int id)
        => base.Delete(id);
}
