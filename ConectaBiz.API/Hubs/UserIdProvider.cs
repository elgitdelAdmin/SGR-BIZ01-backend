using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ConectaBiz.API.Hubs
{
    public class UserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Leemos el mismo Claim que usa CurrentUserService para garantizar que el ID coincide.
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? connection.User?.FindFirst("sub")?.Value
                ?? connection.User?.FindFirst("id")?.Value;
        }
    }
}
