using SvadbeniSalon.Model.Requests;
using SvadbeniSalon.Model.Responses;
using SvadbeniSalon.Model.SearchObjects;
using SvadbeniSalon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SvadbeniSalon.Model.Constants;

namespace SvadbeniSalon.WebAPI.Controllers;

[Authorize(Roles = RoleNames.AdminOrZaposlenik)]
public class UsersController : BaseCRUDController<UserResponse, UserSearch, UserInsertRequest, UserUpdateRequest, IUserService>
{
    public UsersController(IUserService userService) : base(userService)
    {
    }

    [Authorize(Roles = RoleNames.Admin)]
    public override Task<ActionResult<UserResponse>> Create([FromBody] UserInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorize(Roles = RoleNames.Admin)]
    public override Task<ActionResult<UserResponse>> Update(int id, [FromBody] UserUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorize(Roles = RoleNames.Admin)]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPut("{id}/SetPassword")]
    public async Task<IActionResult> SetPassword(int id, [FromBody] AdminSetPasswordRequest request)
    {
        await _service.AdminSetPasswordAsync(id, request);
        return Ok(new { message = "Lozinka korisnika je uspješno postavljena." });
    }
}
