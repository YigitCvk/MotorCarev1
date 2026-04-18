using FluentValidation;

namespace MotorCare.Application.Vehicles.Queries.GetVehicleByPlate;

public class GetVehicleByPlateQueryValidator : AbstractValidator<GetVehicleByPlateQuery>
{
    public GetVehicleByPlateQueryValidator()
    {
        RuleFor(x => x.Plate).NotEmpty().MaximumLength(20);
    }
}
