using Microsoft.EntityFrameworkCore;
using PostTrade.Application.Interfaces;
using PostTrade.Infrastructure.Services;
using PostTrade.Persistence;
using PostTrade.Persistence.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Post-Trade Backoffice API", 
        Version = "v1",
        Description = "Complete broker-grade post-trade processing system"
    });
});

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

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
