using FluentValidation;

namespace MotorCare.Application.Tenants.Commands.CreateTenantWithOwner;

public class CreateTenantWithOwnerCommandValidator : AbstractValidator<CreateTenantWithOwnerCommand>
{
    public CreateTenantWithOwnerCommandValidator()
    {
        RuleFor(x => x.TenantIdentifier).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OwnerFullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OwnerEmail).NotEmpty().MaximumLength(150).EmailAddress();
        RuleFor(x => x.OwnerPassword).NotEmpty().MinimumLength(8).MaximumLength(200);
    }
}
