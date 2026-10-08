using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Services;

namespace SvadbeniSalon.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class NotifikacijeController : ControllerBase
{
    private readonly INotifikacijaService _service;
    private readonly IAuthenticatedUserAccessor _userAccessor;

    public NotifikacijeController(
        INotifikacijaService service,
        IAuthenticatedUserAccessor userAccessor)
    {
        _service = service;
        _userAccessor = userAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotifikacijaResponse>>> GetMine([FromQuery] int? take = 50)
    {
        var userId = RequireUserId();
        return Ok(await _service.GetMineAsync(userId, take));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountResponse>> GetUnreadCount()
    {
        var userId = RequireUserId();
        var count = await _service.GetUnreadCountAsync(userId);
        return Ok(new UnreadCountResponse { Count = count });
    }

    [HttpPut("{id:int}/read")]
    public async Task<ActionResult<NotifikacijaResponse>> MarkAsRead(int id)
    {
        var userId = RequireUserId();
        return Ok(await _service.MarkAsReadAsync(userId, id));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<object>> MarkAllAsRead()
    {
        var userId = RequireUserId();
        var updated = await _service.MarkAllAsReadAsync(userId);
        return Ok(new { updated });
    }

    private int RequireUserId() =>
        _userAccessor.GetUserId()
        ?? throw new UnauthorizedAccessException("Niste prijavljeni.");
}
