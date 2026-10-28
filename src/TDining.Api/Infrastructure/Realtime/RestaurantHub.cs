using Microsoft.AspNetCore.SignalR;

namespace TDining.Api.Infrastructure.Realtime;

public sealed class RestaurantHub : Hub;

/// <summary>Invalidates workspace data only after a successful mutation has committed.</summary>
public sealed class WorkspaceChangedFilter(
    IHubContext<RestaurantHub> hub,
    ILogger<WorkspaceChangedFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);
        if (result is IStatusCodeHttpResult { StatusCode: >= 200 and < 300 })
        {
            try
            {
                // A disconnected caller must not cancel notification of an already committed write.
                await hub.Clients.All.SendAsync("WorkspaceChanged");
            }
            catch (Exception ex)
            {
                // Notification failure must not turn a committed payment/order into a failed request.
                logger.LogWarning(ex, "Could not broadcast workspace change");
            }
        }
        return result;
    }
}
