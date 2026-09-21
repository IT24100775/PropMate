using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.Services.Maintenance;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<ITechnicianService, TechnicianService>();
builder.Services.AddScoped<IMaintenanceAiService, MaintenanceAiService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();