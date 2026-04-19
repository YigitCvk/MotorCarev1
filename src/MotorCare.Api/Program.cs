using System.IO;
using System.Text;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MotorCare.Api.Authorization;
using MotorCare.Api.Middleware;
using MotorCare.Api.Swagger;
using MotorCare.Application;
using MotorCare.Domain.Enums;
using MotorCare.Infrastructure;
using MotorCare.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    var dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
    Directory.CreateDirectory(dataProtectionPath);

    builder.Services.AddDataProtection()
        .SetApplicationName("MotorCare.Api")
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
}

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
if (jwtOptions is not null && !string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<ActiveTenantUserGuardMiddleware>();
app.UseAuthorization();
app.MapCarter();

app.Run();
