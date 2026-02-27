using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
using PostTrade.Application.Common.Behaviors;
using PostTrade.Application.Interfaces;
using PostTrade.Infrastructure.EOD;
using PostTrade.Infrastructure.Services;
using PostTrade.Persistence;
using PostTrade.Persistence.Context;
using PostTrade.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// JSON: serialize enums as strings throughout the API
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// API Explorer + OpenAPI spec (Swashbuckle generates the JSON; Scalar renders it)
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

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(PostTrade.Application.Common.Behaviors.ValidationBehavior<,>).Assembly));

// AutoMapper
builder.Services.AddAutoMapper(typeof(PostTrade.Application.Common.Mappings.MappingProfile).Assembly);

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(PostTrade.Application.Common.Behaviors.ValidationBehavior<,>).Assembly);
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// JWT Authentication
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

// Multi-tenancy
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Database - PostgreSQL
builder.Services.AddDbContext<PostTradeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("PostTrade.Persistence")
    ));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IEODProcessingService, EODProcessingService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health checks (required for Kubernetes liveness/readiness probes)
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    // Swashbuckle generates the OpenAPI JSON spec at /swagger/v1/swagger.json
    app.UseSwagger();

    // Scalar replaces Swagger UI — available at /scalar/v1
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

// Endpoints — Minimal API
app.MapGroup("/api/auth").MapAuthEndpoints();

// Master Setup
app.MapGroup("/api/tenants").MapTenantEndpoints().RequireAuthorization();
app.MapGroup("/api/brokers").MapBrokerEndpoints().RequireAuthorization();
app.MapGroup("/api/clients").MapClientEndpoints().RequireAuthorization();
app.MapGroup("/api/users").MapUserEndpoints().RequireAuthorization();
app.MapGroup("/api/roles").MapRoleEndpoints().RequireAuthorization();
app.MapGroup("/api/exchanges").MapExchangeEndpoints().RequireAuthorization();
app.MapGroup("/api/segments").MapSegmentEndpoints().RequireAuthorization();
app.MapGroup("/api/exchange-segments").MapExchangeSegmentEndpoints().RequireAuthorization();
app.MapGroup("/api/branches").MapBranchEndpoints().RequireAuthorization();
app.MapGroup("/api/clients").MapClientSegmentEndpoints().RequireAuthorization();
app.MapGroup("/api/instruments").MapInstrumentEndpoints().RequireAuthorization();

// Trading
app.MapGroup("/api/trades").MapTradeEndpoints().RequireAuthorization();
app.MapGroup("/api/positions").MapPositionEndpoints().RequireAuthorization();
app.MapGroup("/api/pnl").MapPnLEndpoints().RequireAuthorization();

// Settlement
app.MapSettlementEndpoints();

// Ledger
app.MapLedgerEndpoints();

// Reconciliation
app.MapReconciliationEndpoints();

// Corporate Actions
app.MapCorporateActionEndpoints();

// EOD Processing
app.MapGroup("/api/eod").MapEodEndpoints().RequireAuthorization();

// Health check endpoint — used by Kubernetes liveness and readiness probes
app.MapHealthChecks("/health");

// Apply pending migrations and seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PostTrade.Persistence.Context.PostTradeDbContext>();
    await db.Database.MigrateAsync();
    await PostTrade.Persistence.DatabaseSeeder.SeedAsync(db, BCrypt.Net.BCrypt.HashPassword);
}

app.Run();
