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
public class RacuniController : ControllerBase
{
    private readonly IRacunService _service;

    public RacuniController(IRacunService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<PageResult<RacunResponse>> GetAll([FromQuery] RacunSearchObject? search)
    {
        return await _service.GetAllAsync(search);
    }

    [HttpPost]
    public async Task<ActionResult<RacunResponse>> Create([FromBody] RacunInsertRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result;
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        var pdf = await _service.GetPdfAsync(id);
        var fileName = $"racun-{id}.pdf";
        return File(pdf, "application/pdf", fileName);
    }
}
