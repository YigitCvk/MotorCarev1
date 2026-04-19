namespace MotorCare.App.Models.Customers;

public sealed class CreateCustomerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Whatsapp { get; set; }
    public string? Notes { get; set; }
}
