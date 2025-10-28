using Clenkasoft.DicomMedAppApi.Contracts;
using Clenkasoft.DicomMedAppApi.Endpoints;
using Clenkasoft.DicomMedAppApi.Infrastructure.Data;
using Clenkasoft.DicomMedAppApi.Repositories;
using Clenkasoft.DicomMedAppApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DICOM API", Version = "v1" });
});

// Configure PostgreSQL Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register DICOM Service
builder.Services.AddScoped<IDicomService, DicomService>();
builder.Services.AddScoped<IDicomRepository, DicomRepository>();

var usingCloudStorage = builder.Configuration.GetValue<bool>("UseCloudStorage");
if (usingCloudStorage)
{
    // Register cloud storage service
    builder.Services.AddScoped<IDicomFileService, AzureStorageBlobService>();
}
else
{
    // Register local storage service
    builder.Services.AddScoped<IDicomFileService, LocalDicomFileService>();
}

// Read allowed origins from configuration
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            // When AllowCredentials() is used, origins must be explicit (not AllowAnyOrigin)
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
                  //.AllowCredentials(); Frontend does not send credentials for now
        }
        else
        {
            // Fallback: allow any origin if nothing configured
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");


// Apply database migrations on startup (for development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        // This will create the database and apply migrations
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Database migrations applied successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
    }
}


WeatherEndPoints.Map(app);
PatientsEndPoints.Map(app);
SeriesEndPoints.Map(app);
StudiesEndPoints.Map(app);
FileEndPoints.Map(app);
app.Run();

