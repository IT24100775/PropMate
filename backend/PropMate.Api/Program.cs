using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PropMate.Api.Data;
using PropMate.Api.Services;
using PropMate.Api.Services.Interfaces;
using System.Text;
using System.Text.Json.Serialization;
using PropMate.Api.Services.Maintenance;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("ENVIRONMENT: " + builder.Environment.EnvironmentName);
Console.WriteLine("CONNECTION STRING FOUND: " +
    !string.IsNullOrEmpty(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ----------------------------------------------------
// OpenAPI / Swagger
// ----------------------------------------------------

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ----------------------------------------------------
// Controllers + JSON Enum Configuration
// ----------------------------------------------------

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// ----------------------------------------------------
// Database
// ----------------------------------------------------

// Component 1 database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ----------------------------------------------------
// Application Services
// ----------------------------------------------------

builder.Services.AddScoped<IPropertyListingService, PropertyListingService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Component 3 - Applications, Offers & Transactions
builder.Services.AddScoped<ITransactionService, TransactionService>();

// ----------------------------------------------------
// JWT Authentication
// ----------------------------------------------------

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT key is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ----------------------------------------------------
// Property Verification AI Service
// ----------------------------------------------------

builder.Services.AddHttpClient<
    IPropertyVerificationClient,
    PropertyVerificationClient>(
    client =>
    {
        var baseUrl = builder.Configuration[
            "PropertyVerificationService:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "Property verification service BaseUrl is not configured.");
        }

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(15);
    });

    // Component 3 - Transaction Negotiation Agent
builder.Services.AddHttpClient<
    IAgentWorkflowClient, 
    AgentWorkflowClient>(
    client =>
    {
        var baseUrl = builder.Configuration["AgenticAiService:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "Agentic AI service BaseUrl is not configured.");
        }

        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(60);
    });


// Component 4 - Maintenance Management
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<ITechnicianService, TechnicianService>();
builder.Services.AddScoped<IMaintenanceAiService, MaintenanceAiService>();

// ----------------------------------------------------
// CORS
// ----------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ----------------------------------------------------
// Build Application
// ----------------------------------------------------

var app = builder.Build();

// ----------------------------------------------------
// Development Tools
// ----------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ----------------------------------------------------
// HTTP Pipeline
// ----------------------------------------------------

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ----------------------------------------------------
// Health Check
// ----------------------------------------------------

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "propmate-api"
}));

// ----------------------------------------------------
// Predefined Property Manager Account
// ----------------------------------------------------

await PropertyManagerSeeder.SeedAsync(
    app.Services,
    app.Configuration);

app.Run();