using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class PreporukeController : ControllerBase
{
    private readonly IPreporukaService _preporukaService;
    private readonly IAuthenticatedUserAccessor _userAccessor;

    public PreporukeController(
        IPreporukaService preporukaService,
        IAuthenticatedUserAccessor userAccessor)
    {
        _preporukaService = preporukaService;
        _userAccessor = userAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<List<PreporukaResponse>>> Get([FromQuery] int limit = 5)
    {
        var userId = _userAccessor.GetUserId()
                     ?? throw new UnauthorizedAccessException("Niste prijavljeni.");

        var items = await _preporukaService.GetRecommendationsAsync(userId, limit);
        return Ok(items);
    }
}
