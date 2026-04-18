using FluentValidation;

namespace MotorCare.Application.ServiceOrders.Commands.RemoveOperationFromOrder;

public class RemoveOperationFromOrderCommandValidator : AbstractValidator<RemoveOperationFromOrderCommand>
{
    public RemoveOperationFromOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.OperationId).NotEmpty();
    }
}
