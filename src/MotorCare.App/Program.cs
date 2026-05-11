using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
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

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<BuildInfoOptions>(builder.Configuration.GetSection(BuildInfoOptions.SectionName));
builder.Services.Configure<PublicReportOptions>(builder.Configuration.GetSection(PublicReportOptions.SectionName));
builder.Services.Configure<SeoOptions>(builder.Configuration.GetSection(SeoOptions.SectionName));

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
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PublicRecordsService>();
builder.Services.AddScoped<QrCodeSvgService>();
builder.Services.AddScoped<ImportsService>();

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

app.MapGet("/robots.txt", (IOptions<SeoOptions> seoOptions) =>
{
    var seo = seoOptions.Value;
    var builder = new StringBuilder();
    builder.AppendLine("User-agent: *");

    if (seo.RobotsIndexEnabled)
    {
        builder.AppendLine("Allow: /");
    }
    else
    {
        builder.AppendLine("Disallow: /");
        builder.AppendLine("# Staging/default: indexing is disabled until production domain and HTTPS are ready.");
    }

    builder.AppendLine("Disallow: /dashboard");
    builder.AppendLine("Disallow: /customers");
    builder.AppendLine("Disallow: /vehicles");
    builder.AppendLine("Disallow: /appointments");
    builder.AppendLine("Disallow: /service-orders");
    builder.AppendLine("Disallow: /inspections");
    builder.AppendLine("Disallow: /imports");
    builder.AppendLine("Disallow: /settings");
    builder.AppendLine("Disallow: /login");
    builder.AppendLine("Disallow: /register");
    builder.AppendLine("Disallow: /forgot-password");
    builder.AppendLine("Disallow: /reset-password");
    builder.AppendLine("Disallow: /two-factor");
    builder.AppendLine("Disallow: /api/");
    builder.AppendLine("Disallow: /public/service-record/");
    builder.AppendLine("Disallow: /public/inspection-report/");
    builder.AppendLine($"Sitemap: {ToAbsoluteSeoUrl(seo, "/sitemap.xml")}");

    return Results.Text(builder.ToString(), "text/plain; charset=utf-8");
});

app.MapGet("/sitemap.xml", (IOptions<SeoOptions> seoOptions) =>
{
    var seo = seoOptions.Value;
    var landingUrl = ToAbsoluteSeoUrl(seo, "/");
    var xml = $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
          <url>
            <loc>{System.Security.SecurityElement.Escape(landingUrl)}</loc>
            <changefreq>weekly</changefreq>
            <priority>1.0</priority>
          </url>
        </urlset>
        """;

    return Results.Text(xml, "application/xml; charset=utf-8");
});

app.MapGet("/attachment-proxy/service-orders/{id:guid}/attachments/{attachmentId:guid}/download", async (
    Guid id,
    Guid attachmentId,
    HttpContext context,
    HttpClient httpClient,
    CancellationToken cancellationToken) =>
{
    var apiPath = $"/api/service-orders/{id:D}/attachments/{attachmentId:D}/download{context.Request.QueryString}";
    using var message = new HttpRequestMessage(HttpMethod.Get, apiPath);

    if (context.Request.Headers.TryGetValue("Authorization", out var authorization) &&
        AuthenticationHeaderValue.TryParse(authorization.ToString(), out var authorizationHeader))
    {
        message.Headers.Authorization = authorizationHeader;
    }

    using var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        return Results.StatusCode((int)response.StatusCode);
    }

    var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
    var download = context.Request.Query.TryGetValue("download", out var downloadValue) &&
                   bool.TryParse(downloadValue.ToString(), out var parsedDownload) &&
                   parsedDownload;
    var fileName = download
        ? response.Content.Headers.ContentDisposition?.FileNameStar ??
          response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
        : null;

    return Results.File(bytes, contentType, fileName);
});

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static string ToAbsoluteSeoUrl(SeoOptions seo, string path)
{
    if (Uri.TryCreate(path, UriKind.Absolute, out var absolute))
    {
        return absolute.ToString();
    }

    var baseUrl = string.IsNullOrWhiteSpace(seo.SiteBaseUrl)
        ? "http://46.225.166.254"
        : seo.SiteBaseUrl.Trim().TrimEnd('/');
    var normalizedPath = string.IsNullOrWhiteSpace(path)
        ? "/"
        : path.StartsWith('/') ? path : "/" + path;

    return baseUrl + normalizedPath;
}
