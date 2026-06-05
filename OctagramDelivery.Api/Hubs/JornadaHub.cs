using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OctagramDelivery.Api.Hubs;

[Authorize]
public class JornadaHub : Hub
{
    public async Task JoinNegocioGroup(int negocioId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"negocio-{negocioId}");
    }

    public async Task JoinJornadaGroup(int jornadaId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"jornada-{jornadaId}");
    }

    public async Task LeaveNegocioGroup(int negocioId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"negocio-{negocioId}");
    }
}
