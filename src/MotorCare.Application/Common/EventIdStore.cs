using Microsoft.Extensions.Logging;

namespace MotorCare.Application.Common;

/// <summary>
/// Centralised registry of structured log event identifiers.
/// Numeric ranges ensure non-overlapping IDs across domains for Kibana filtering.
/// </summary>
public static class EventIdStore
{
    // ── Common  1000-1099 ───────────────────────────────────────────────────
    public static class Common
    {
        public static readonly EventId RequestStarted      = new(1000, nameof(RequestStarted));
        public static readonly EventId RequestCompleted    = new(1001, nameof(RequestCompleted));
        public static readonly EventId UnhandledException  = new(1002, nameof(UnhandledException));
        public static readonly EventId ValidationFailed    = new(1003, nameof(ValidationFailed));
    }

    // ── Auth  1100-1199 ─────────────────────────────────────────────────────
    public static class Auth
    {
        public static readonly EventId LoginAttempt    = new(1100, nameof(LoginAttempt));
        public static readonly EventId LoginSucceeded  = new(1101, nameof(LoginSucceeded));
        public static readonly EventId LoginFailed     = new(1102, nameof(LoginFailed));
        public static readonly EventId TokenRefreshed  = new(1103, nameof(TokenRefreshed));
        public static readonly EventId LogoutSucceeded = new(1104, nameof(LogoutSucceeded));
    }

    // ── Customer  1200-1299 ─────────────────────────────────────────────────
    public static class Customer
    {
        public static readonly EventId CustomerCreated       = new(1200, nameof(CustomerCreated));
        public static readonly EventId CustomerUpdated       = new(1201, nameof(CustomerUpdated));
        public static readonly EventId CustomerFetched       = new(1202, nameof(CustomerFetched));
        public static readonly EventId CustomerSummaryFetched = new(1203, nameof(CustomerSummaryFetched));
    }

    // ── Vehicle  1300-1399 ──────────────────────────────────────────────────
    public static class Vehicle
    {
        public static readonly EventId VehicleCreated       = new(1300, nameof(VehicleCreated));
        public static readonly EventId VehicleUpdated       = new(1301, nameof(VehicleUpdated));
        public static readonly EventId VehicleFetched       = new(1302, nameof(VehicleFetched));
        public static readonly EventId VehicleLookupByPlate = new(1303, nameof(VehicleLookupByPlate));
    }

    // ── Appointment  1400-1499 ──────────────────────────────────────────────
    public static class Appointment
    {
        public static readonly EventId AppointmentCreated              = new(1400, nameof(AppointmentCreated));
        public static readonly EventId AppointmentUpdated              = new(1401, nameof(AppointmentUpdated));
        public static readonly EventId AppointmentStatusUpdated        = new(1402, nameof(AppointmentStatusUpdated));
        public static readonly EventId AppointmentConvertedToServiceOrder = new(1403, nameof(AppointmentConvertedToServiceOrder));
        public static readonly EventId AppointmentFetched              = new(1404, nameof(AppointmentFetched));
    }

    // ── ServiceOrder  1500-1599 ─────────────────────────────────────────────
    public static class ServiceOrder
    {
        public static readonly EventId ServiceOrderCreated       = new(1500, nameof(ServiceOrderCreated));
        public static readonly EventId ServiceOrderFetched       = new(1501, nameof(ServiceOrderFetched));
        public static readonly EventId ServiceOrderStatusUpdated = new(1502, nameof(ServiceOrderStatusUpdated));
        public static readonly EventId OperationAdded            = new(1503, nameof(OperationAdded));
        public static readonly EventId OperationRemoved          = new(1504, nameof(OperationRemoved));
        public static readonly EventId PartAdded                 = new(1505, nameof(PartAdded));
        public static readonly EventId PartRemoved               = new(1506, nameof(PartRemoved));
        public static readonly EventId PaymentAdded              = new(1507, nameof(PaymentAdded));
        public static readonly EventId DiscountUpdated           = new(1508, nameof(DiscountUpdated));
        public static readonly EventId BusinessRuleBlocked       = new(1509, nameof(BusinessRuleBlocked));
    }

    // ── Inspection  1600-1699 ───────────────────────────────────────────────
    public static class Inspection
    {
        public static readonly EventId InspectionCreated     = new(1600, nameof(InspectionCreated));
        public static readonly EventId InspectionFetched     = new(1601, nameof(InspectionFetched));
        public static readonly EventId InspectionItemUpdated = new(1602, nameof(InspectionItemUpdated));
        public static readonly EventId InspectionCompleted   = new(1603, nameof(InspectionCompleted));
        public static readonly EventId InspectionCancelled   = new(1604, nameof(InspectionCancelled));
    }

    // ── ServiceCatalog  1700-1799 ───────────────────────────────────────────
    public static class ServiceCatalog
    {
        public static readonly EventId ServiceCatalogItemCreated     = new(1700, nameof(ServiceCatalogItemCreated));
        public static readonly EventId ServiceCatalogItemUpdated     = new(1701, nameof(ServiceCatalogItemUpdated));
        public static readonly EventId ServiceCatalogItemActivated   = new(1702, nameof(ServiceCatalogItemActivated));
        public static readonly EventId ServiceCatalogItemDeactivated = new(1703, nameof(ServiceCatalogItemDeactivated));
    }

    // ── Inventory  1800-1899 ────────────────────────────────────────────────
    public static class Inventory
    {
        public static readonly EventId InventoryItemCreated  = new(1800, nameof(InventoryItemCreated));
        public static readonly EventId InventoryItemUpdated  = new(1801, nameof(InventoryItemUpdated));
        public static readonly EventId InventoryStockAdjusted = new(1802, nameof(InventoryStockAdjusted));
        public static readonly EventId LowStockDetected      = new(1803, nameof(LowStockDetected));
    }

    // ── Dashboard  1900-1999 ────────────────────────────────────────────────
    public static class Dashboard
    {
        public static readonly EventId DailySummaryFetched = new(1900, nameof(DailySummaryFetched));
    }
}
