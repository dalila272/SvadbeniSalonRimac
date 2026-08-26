using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Model.Constants;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.WebAPI.Controllers;

[Authorize(Roles = RoleNames.AdminOrZaposlenik)]
[ApiController]
[Route("[controller]")]
public class IzvjestajiController : ControllerBase
{
    private readonly IIzvjestajService _service;

    public IzvjestajiController(IIzvjestajService service)
    {
        _service = service;
    }

    [HttpGet("svadbe/pdf")]
    public async Task<IActionResult> SvadbePdf([FromQuery] SvadbeIzvjestajSearchObject search)
    {
        var pdf = await _service.GetSvadbePdfAsync(search);
        var fileName =
            $"svadbe-{search.DatumOd:yyyyMMdd}-{search.DatumDo:yyyyMMdd}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    [HttpGet("uplate/pdf")]
    public async Task<IActionResult> UplatePdf([FromQuery] UplateIzvjestajSearchObject search)
    {
        var pdf = await _service.GetUplatePdfAsync(search);
        var fileName =
            $"uplate-{search.DatumOd:yyyyMMdd}-{search.DatumDo:yyyyMMdd}.pdf";
        return File(pdf, "application/pdf", fileName);
    }
}
