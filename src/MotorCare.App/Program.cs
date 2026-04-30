using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using MotorCare.App.Configuration;
using MotorCare.App.Components;
using MotorCare.App.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    [$"{BuildInfoOptions.SectionName}:CommitSha"] = Environment.GetEnvironmentVariable("MOTORCARE_COMMIT_SHA") ?? builder.Configuration[$"{BuildInfoOptions.SectionName}:CommitSha"],
    [$"{BuildInfoOptions.SectionName}:BuildTime"] = Environment.GetEnvironmentVariable("MOTORCARE_BUILD_TIME") ?? builder.Configuration[$"{BuildInfoOptions.SectionName}:BuildTime"]
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = 1;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<BuildInfoOptions>(builder.Configuration.GetSection(BuildInfoOptions.SectionName));

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

var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"];
if (string.IsNullOrWhiteSpace(dataProtectionPath))
{
    dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
}

Directory.CreateDirectory(dataProtectionPath);

builder.Services.AddDataProtection()
    .SetApplicationName("MotorCare.App")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

if (Directory.Exists(app.Environment.WebRootPath))
{
    app.UseStaticFiles();
}

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
