using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SvadbeniSalon.WebAPI.Services.AccessManager;

namespace SvadbeniSalon.WebAPI.Hubs;

[Authorize]
public class NotificationHub : Hub
{
}

public class ClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirst(ClaimNames.Id)?.Value;
}
