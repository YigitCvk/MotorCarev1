using MotorCare.Application.Common.Security;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.UnitTests.Security;

public class RoleCapabilitiesTests
{
    [Fact]
    public void Technician_Cannot_AddPayment()
    {
        RoleCapabilities.ServiceOrderPayments.Should().NotContain(UserRole.Technician);
    }

    [Fact]
    public void ReadOnly_Cannot_CreateServiceOrder()
    {
        RoleCapabilities.ServiceOrderWrite.Should().NotContain(UserRole.ReadOnly);
    }

    [Fact]
    public void Inspector_Cannot_UpdatePayment()
    {
        RoleCapabilities.ServiceOrderPayments.Should().NotContain(UserRole.Inspector);
    }

    [Fact]
    public void Owner_Can_InviteUser()
    {
        RoleCapabilities.UserManagement.Should().Contain(UserRole.Owner);
    }
}
