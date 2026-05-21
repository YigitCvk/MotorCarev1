using MotorCare.Application.Common.Security;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.UnitTests.Security;

public class RoleCapabilitiesTests
{
    public static TheoryData<string, IReadOnlyCollection<UserRole>, UserRole[]> CapabilityMatrix => new()
    {
        { nameof(RoleCapabilities.UserManagement), RoleCapabilities.UserManagement, [UserRole.Owner, UserRole.Admin] },
        { nameof(RoleCapabilities.CustomerRead), RoleCapabilities.CustomerRead, [UserRole.Owner, UserRole.Admin, UserRole.Receptionist, UserRole.Technician, UserRole.Inspector, UserRole.Accountant, UserRole.ReadOnly, UserRole.Manager] },
        { nameof(RoleCapabilities.CustomerWrite), RoleCapabilities.CustomerWrite, [UserRole.Owner, UserRole.Admin, UserRole.Receptionist] },
        { nameof(RoleCapabilities.ServiceOrderRead), RoleCapabilities.ServiceOrderRead, [UserRole.Owner, UserRole.Admin, UserRole.Receptionist, UserRole.Technician, UserRole.Accountant, UserRole.ReadOnly, UserRole.Manager] },
        { nameof(RoleCapabilities.ServiceOrderWrite), RoleCapabilities.ServiceOrderWrite, [UserRole.Owner, UserRole.Admin, UserRole.Receptionist, UserRole.Technician] },
        { nameof(RoleCapabilities.ServiceOrderPayments), RoleCapabilities.ServiceOrderPayments, [UserRole.Owner, UserRole.Admin, UserRole.Receptionist, UserRole.Accountant] },
        { nameof(RoleCapabilities.InspectionRead), RoleCapabilities.InspectionRead, [UserRole.Owner, UserRole.Admin, UserRole.Receptionist, UserRole.Inspector, UserRole.ReadOnly, UserRole.Manager] },
        { nameof(RoleCapabilities.InspectionWrite), RoleCapabilities.InspectionWrite, [UserRole.Owner, UserRole.Admin, UserRole.Inspector] },
        { nameof(RoleCapabilities.InventoryRead), RoleCapabilities.InventoryRead, [UserRole.Owner, UserRole.Admin, UserRole.Receptionist, UserRole.Technician, UserRole.ReadOnly, UserRole.Manager] },
        { nameof(RoleCapabilities.InventoryWrite), RoleCapabilities.InventoryWrite, [UserRole.Owner, UserRole.Admin, UserRole.Receptionist] },
        { nameof(RoleCapabilities.DashboardRead), RoleCapabilities.DashboardRead, [UserRole.Owner, UserRole.Admin, UserRole.Receptionist, UserRole.Accountant, UserRole.ReadOnly, UserRole.Manager] },
        { nameof(RoleCapabilities.ImportOperations), RoleCapabilities.ImportOperations, [UserRole.Owner, UserRole.Admin] }
    };

    [Theory]
    [MemberData(nameof(CapabilityMatrix))]
    public void CapabilitySet_MatchesExpectedRoles(
        string capabilityName,
        IReadOnlyCollection<UserRole> actualRoles,
        UserRole[] expectedRoles)
    {
        actualRoles.Should().BeEquivalentTo(expectedRoles, because: $"{capabilityName} drives API authorization policies");
    }

    [Theory]
    [MemberData(nameof(CapabilityMatrix))]
    public void CapabilitySet_DoesNotContainDuplicateRoles(
        string _,
        IReadOnlyCollection<UserRole> actualRoles,
        UserRole[] expectedRoles)
    {
        expectedRoles.Should().NotBeEmpty();
        actualRoles.Should().OnlyHaveUniqueItems();
    }

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

    [Fact]
    public void ReadOnly_HasNoWriteCapabilities()
    {
        var writeCapabilities = new[]
        {
            RoleCapabilities.UserManagement,
            RoleCapabilities.CustomerWrite,
            RoleCapabilities.ServiceOrderWrite,
            RoleCapabilities.ServiceOrderPayments,
            RoleCapabilities.InspectionWrite,
            RoleCapabilities.InventoryWrite,
            RoleCapabilities.ImportOperations
        };

        writeCapabilities.Should().OnlyContain(capability => !capability.Contains(UserRole.ReadOnly));
    }

    [Fact]
    public void Names_ReturnsRoleNames_ForPolicyRegistration()
    {
        RoleCapabilities.Names([UserRole.Owner, UserRole.Admin])
            .Should()
            .Equal("Owner", "Admin");
    }
}
