namespace MotorCare.Application.Common.Errors;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string UnexpectedError = "UNEXPECTED_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string DomainRuleViolation = "DOMAIN_RULE_VIOLATION";

    public const string LoginFailed = "LOGIN_FAILED";
    public const string EmailNotVerified = "EMAIL_NOT_VERIFIED";
    public const string UserInactive = "USER_INACTIVE";
    public const string TenantInactive = "TENANT_INACTIVE";
    public const string TooManyAttempts = "TOO_MANY_ATTEMPTS";

    public const string RegisterFailed = "REGISTER_FAILED";
    public const string EmailVerificationFailed = "EMAIL_VERIFICATION_FAILED";
    public const string EmailVerificationCodeInvalid = "EMAIL_VERIFICATION_CODE_INVALID";
    public const string EmailVerificationCodeExpired = "EMAIL_VERIFICATION_CODE_EXPIRED";
    public const string EmailAlreadyVerified = "EMAIL_ALREADY_VERIFIED";

    public const string PasswordResetFailed = "PASSWORD_RESET_FAILED";
    public const string InviteInvalid = "INVITE_INVALID";
    public const string InviteExpired = "INVITE_EXPIRED";
    public const string InviteAlreadyUsed = "INVITE_ALREADY_USED";

    public const string CustomerOperationFailed = "CUSTOMER_OPERATION_FAILED";
    public const string VehicleOperationFailed = "VEHICLE_OPERATION_FAILED";
    public const string ServiceOrderOperationFailed = "SERVICE_ORDER_OPERATION_FAILED";
    public const string PaymentOperationFailed = "PAYMENT_OPERATION_FAILED";
    public const string InspectionOperationFailed = "INSPECTION_OPERATION_FAILED";
    public const string InventoryOperationFailed = "INVENTORY_OPERATION_FAILED";
}
