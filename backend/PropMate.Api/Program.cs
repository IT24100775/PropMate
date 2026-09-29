using Microsoft.EntityFrameworkCore;
using PropMate.Api.Data;
using PropMate.Api.Services;
using PropMate.Api.Services.Interfaces;
using PropMate.Api.Services.Maintenance;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPropertyListingService, PropertyListingService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<ITechnicianService, TechnicianService>();
builder.Services.AddScoped<IMaintenanceAiService, MaintenanceAiService>();

builder.Services.AddHttpClient<IPropertyVerificationClient, PropertyVerificationClient>(client =>
{
    var baseUrl = builder.Configuration["PropertyVerificationService:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Property verification service BaseUrl is not configured.");

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddHttpClient<IAgentWorkflowClient, AgentWorkflowClient>(client =>
{
    var baseUrl = builder.Configuration["AgenticAiService:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Agentic AI service BaseUrl is not configured.");

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost:"))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        appDb.Database.Migrate();
        var applicationDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        applicationDb.Database.Migrate();

        DbInitializer.SeedDemoData(appDb);
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
