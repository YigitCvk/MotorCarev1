using MotorCare.App.Components;
using MotorCare.App.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7278";

builder.Services.AddScoped(sp =>
{
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    var handler = new HttpClientHandler();

    if (environment.IsDevelopment() &&
        Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri) &&
        string.Equals(apiUri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
});

builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ServiceOrdersService>();
builder.Services.AddScoped<CustomersService>();
builder.Services.AddScoped<VehiclesService>();
builder.Services.AddScoped<AppointmentsService>();
builder.Services.AddScoped<ServicesService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<InspectionsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
