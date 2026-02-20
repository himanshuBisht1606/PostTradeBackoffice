using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using PostTrade.API;
using PostTrade.API.Features.Auth;
using PostTrade.API.Features.MasterSetup;
using PostTrade.API.Features.CorporateActions;
using PostTrade.API.Features.EOD;
using PostTrade.API.Features.Ledger;
using PostTrade.API.Features.Reconciliation;
using PostTrade.API.Features.Settlement;
using PostTrade.API.Features.Trading;
using PostTrade.API.Middleware;
using Scalar.AspNetCore;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using PostTrade.Application.Common.Behaviors;
using PostTrade.Application.Interfaces;
using PostTrade.Infrastructure.EOD;
using PostTrade.Infrastructure.Services;
using PostTrade.Persistence;
using PostTrade.Persistence.Context;
using PostTrade.Persistence.Repositories;

// ── Serilog bootstrap logger (captures startup errors before full config) ─────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
var builder = WebApplication.CreateBuilder(args);

// ── Serilog — structured logging sent to SigNoz via OTLP ─────────────────────
var otlpEndpoint = builder.Configuration["OpenTelemetry:Endpoint"];

builder.Host.UseSerilog((ctx, services, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext()
       .Enrich.WithMachineName()
       .Enrich.WithProcessId()
       .Enrich.WithThreadId()
       .WriteTo.Console(
           outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");

    // Only ship to SigNoz when an endpoint is configured (skipped in unit/integration tests)
    if (!string.IsNullOrEmpty(otlpEndpoint))
    {
        cfg.WriteTo.OpenTelemetry(o =>
        {
            o.Endpoint = otlpEndpoint.Replace("4317", "4318"); // Serilog sink uses HTTP, not gRPC
            o.Protocol = OtlpProtocol.HttpProtobuf;
            o.ResourceAttributes = new Dictionary<string, object>
            {
                ["service.name"]             = "PostTrade.API",
                ["service.version"]          = "1.0.0",
                ["deployment.environment"]   = ctx.HostingEnvironment.EnvironmentName
            };
        });
    }
});

// ── OpenTelemetry — traces + metrics → SigNoz via OTLP ───────────────────────
if (!string.IsNullOrEmpty(otlpEndpoint))
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r
            .AddService(
                serviceName:    "PostTrade.API",
                serviceVersion: "1.0.0"))
        .WithTracing(t => t
            .AddAspNetCoreInstrumentation(o =>
            {
                o.RecordException = true;
                // Skip noisy health-check traces
                o.Filter = ctx => ctx.Request.Path != "/health";
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(o =>
            {
                o.SetDbStatementForText = true;  // capture SQL queries in traces
            })
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
        .WithMetrics(m => m
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));
}

// ── Serialize enums as string names (e.g. "Pending" not 1) ───────────────────
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ── API Explorer + OpenAPI (Swashbuckle generates JSON; Scalar renders it) ────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Post-Trade Backoffice API",
        Version = "v1",
        Description = "Complete broker-grade post-trade processing system"
    });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── MediatR ───────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(PostTrade.Application.Common.Behaviors.ValidationBehavior<,>).Assembly));

// ── AutoMapper ────────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(PostTrade.Application.Common.Mappings.MappingProfile).Assembly);

// ── FluentValidation ──────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssembly(typeof(PostTrade.Application.Common.Behaviors.ValidationBehavior<,>).Assembly);
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── Multi-tenancy ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<ITenantContext, TenantContext>();

// ── Database — PostgreSQL ─────────────────────────────────────────────────────
builder.Services.AddDbContext<PostTradeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("PostTrade.Persistence")
    ));

// ── Unit of Work + Repositories ───────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

// ── Domain Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IEODProcessingService, EODProcessingService>();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ── Health checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Post-Trade Backoffice API")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               .WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });
}

app.UseExceptionHandling();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseTenantContext();
app.UseAuthorization();

// ── Endpoints — Minimal API ───────────────────────────────────────────────────
app.MapGroup("/api/auth").MapAuthEndpoints();

app.MapGroup("/api/tenants").MapTenantEndpoints().RequireAuthorization();
app.MapGroup("/api/brokers").MapBrokerEndpoints().RequireAuthorization();
app.MapGroup("/api/clients").MapClientEndpoints().RequireAuthorization();
app.MapGroup("/api/users").MapUserEndpoints().RequireAuthorization();
app.MapGroup("/api/roles").MapRoleEndpoints().RequireAuthorization();
app.MapGroup("/api/exchanges").MapExchangeEndpoints().RequireAuthorization();
app.MapGroup("/api/segments").MapSegmentEndpoints().RequireAuthorization();
app.MapGroup("/api/instruments").MapInstrumentEndpoints().RequireAuthorization();

app.MapGroup("/api/trades").MapTradeEndpoints().RequireAuthorization();
app.MapGroup("/api/positions").MapPositionEndpoints().RequireAuthorization();
app.MapGroup("/api/pnl").MapPnLEndpoints().RequireAuthorization();

app.MapSettlementEndpoints();
app.MapLedgerEndpoints();
app.MapReconciliationEndpoints();
app.MapCorporateActionEndpoints();

app.MapGroup("/api/eod").MapEodEndpoints().RequireAuthorization();
app.MapHealthChecks("/health");

// ── Seed test data in Development ─────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    var seederLogger = app.Services.GetRequiredService<ILogger<Program>>();
    await DatabaseSeeder.SeedAsync(app.Services, seederLogger);
}

app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException is thrown by WebApplicationFactory during integration
    // test teardown — it is a normal shutdown signal, not an error.
    Log.Fatal(ex, "PostTrade.API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
