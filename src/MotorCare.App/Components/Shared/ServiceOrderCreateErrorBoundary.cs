using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MotorCare.Application.Common;

namespace MotorCare.App.Components.Shared;

public sealed class ServiceOrderCreateErrorBoundary : ErrorBoundary
{
    [Inject] private ILogger<ServiceOrderCreateErrorBoundary> Logger { get; set; } = default!;

    protected override Task OnErrorAsync(Exception exception)
    {
        Logger.LogError(
            EventIdStore.ServiceOrder.ServiceOrderCreateRenderFailed,
            exception,
            "Service order create render failed.");

        return Task.CompletedTask;
    }
}
