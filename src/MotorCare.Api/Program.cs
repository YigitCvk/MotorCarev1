using System.IO;
using System.Text;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MotorCare.Api.Authorization;
using MotorCare.Api.Configuration;
using MotorCare.Api.Logging;
using MotorCare.Api.Middleware;
using MotorCare.Api.Swagger;
using MotorCare.Application;
using MotorCare.Domain.Enums;
using MotorCare.Infrastructure;
using MotorCare.Infrastructure.Persistence.Seed;
using MotorCare.Infrastructure.Security;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MotorCare API");

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

    builder.Host.UseSerilog((ctx, services, cfg) =>
    {
        var env = ctx.HostingEnvironment;

        cfg
            .ReadFrom.Configuration(ctx.Configuration, sectionName: "Serilog")
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ApplicationName", "MotorCare.Api")
            .Enrich.WithProperty("Environment", env.EnvironmentName)
            .Enrich.With(new EventNameEnricher())
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{EventId.Name}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        var elasticUri = ctx.Configuration["Elastic:Uri"];
        if (!string.IsNullOrWhiteSpace(elasticUri))
        {
            var indexFormat = ctx.Configuration["Elastic:IndexFormat"] ?? "motorcare-api-{0:yyyy.MM}";
            var username = ctx.Configuration["Elastic:Username"];
            var password = ctx.Configuration["Elastic:Password"];

            var sinkOptions = new ElasticsearchSinkOptions(new Uri(elasticUri))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                IndexFormat = indexFormat,
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
                FailureCallback = e => Console.Error.WriteLine($"[Elastic sink failure] {e.MessageTemplate}"),
                MinimumLogEventLevel = LogEventLevel.Information,
                NumberOfReplicas = 1,
                NumberOfShards = 2,
                ModifyConnectionSettings = conn =>
                {
                    if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                    {
                        conn.BasicAuthentication(username, password);
                    }

                    return conn;
                }
            };

            cfg.WriteTo.Elasticsearch(sinkOptions);
        }
    });

    var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"];
    if (string.IsNullOrWhiteSpace(dataProtectionPath))
    {
        dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
    }

    Directory.CreateDirectory(dataProtectionPath);

    builder.Services.AddDataProtection()
        .SetApplicationName("MotorCare.Api")
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));

    builder.Services.Configure<BuildInfoOptions>(builder.Configuration.GetSection(BuildInfoOptions.SectionName));

    builder.Services.AddCarter();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the JWT access token here. Example: Bearer eyJ..."
        });

        options.OperationFilter<AuthorizeOperationFilter>();
    });

    var authenticationBuilder = builder.Services.AddAuthentication();

    var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
    if (jwtOptions is not null && !string.IsNullOrWhiteSpace(jwtOptions.Key))
    {
        authenticationBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });
    }

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(AuthorizationPolicies.OwnerOnly, policy =>
            policy.RequireRole(UserRole.Owner.ToString()));

        options.AddPolicy(AuthorizationPolicies.TenantManagement, policy =>
            policy.RequireRole(UserRole.Owner.ToString()));

        options.AddPolicy(AuthorizationPolicies.CustomerOperations, policy =>
            policy.RequireRole(
                UserRole.Owner.ToString(),
                UserRole.Admin.ToString(),
                UserRole.Receptionist.ToString()));

        options.AddPolicy(AuthorizationPolicies.ServiceOrderRead, policy =>
            policy.RequireRole(
                UserRole.Owner.ToString(),
                UserRole.Admin.ToString(),
                UserRole.Receptionist.ToString(),
                UserRole.Technician.ToString()));

        options.AddPolicy(AuthorizationPolicies.ServiceOrderWrite, policy =>
            policy.RequireRole(
                UserRole.Owner.ToString(),
                UserRole.Admin.ToString(),
                UserRole.Receptionist.ToString(),
                UserRole.Technician.ToString()));

        options.AddPolicy(AuthorizationPolicies.ServiceOrderPayments, policy =>
            policy.RequireRole(
                UserRole.Owner.ToString(),
                UserRole.Admin.ToString(),
                UserRole.Receptionist.ToString()));

        options.AddPolicy(AuthorizationPolicies.DashboardRead, policy =>
            policy.RequireRole(
                UserRole.Owner.ToString(),
                UserRole.Admin.ToString(),
                UserRole.Receptionist.ToString()));
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<MotorcycleModelCatalogSeeder>();
        await seeder.SeedAsync();
    }

    app.UseForwardedHeaders();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        options.GetLevel = static (ctx, elapsed, ex) =>
            ctx.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : ctx.Response.StatusCode >= 400
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;

        options.EnrichDiagnosticContext = static (diag, ctx) =>
        {
            diag.Set("RequestHost", ctx.Request.Host.Value);
            diag.Set("RequestScheme", ctx.Request.Scheme);
            diag.Set("RequestId", ctx.TraceIdentifier);
            diag.Set("CorrelationId", ctx.Response.Headers["X-Correlation-Id"].ToString());
        };
    });

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseHttpsRedirection();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseAuthentication();
    app.UseMiddleware<UserContextLoggingMiddleware>();

    app.UseMiddleware<ActiveTenantUserGuardMiddleware>();
    app.UseAuthorization();
    app.MapCarter();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "MotorCare API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
