using FluentValidation;

namespace MotorCare.Application.Vehicles.Queries.GetVehiclesByCustomerId;

public sealed class GetVehiclesByCustomerIdQueryValidator : AbstractValidator<GetVehiclesByCustomerIdQuery>
{
    public GetVehiclesByCustomerIdQueryValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
