namespace MotorCare.Domain.Enums;

public enum ServiceOrderStatus
{
    Open = 1,
    InProgress = 2,
    WaitingForParts = 3,
    // Completed is presented in the UI as "Teslime Hazir" until the order is delivered.
    Completed = 4,
    Cancelled = 5,
    Delivered = 6
}
