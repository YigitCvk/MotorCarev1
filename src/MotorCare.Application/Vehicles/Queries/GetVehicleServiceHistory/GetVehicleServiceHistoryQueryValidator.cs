using FluentValidation;

namespace MotorCare.Application.Vehicles.Queries.GetVehicleServiceHistory;

public sealed class GetVehicleServiceHistoryQueryValidator : AbstractValidator<GetVehicleServiceHistoryQuery>
{
    public GetVehicleServiceHistoryQueryValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
    }
}
