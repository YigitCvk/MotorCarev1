namespace MotorCare.App.Models.Customers;

public sealed class CustomerLookupResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Whatsapp { get; set; }
    public string? Notes { get; set; }
}
