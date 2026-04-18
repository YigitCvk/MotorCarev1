using Carter;

namespace MotorCare.Api.Modules;

public sealed class HealthModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "Healthy",
            service = "MotorCare.Api"
        }))
        .WithTags("Health")
        .WithName("GetHealth")
        .WithOpenApi()
        .Produces(StatusCodes.Status200OK);
    }
}
