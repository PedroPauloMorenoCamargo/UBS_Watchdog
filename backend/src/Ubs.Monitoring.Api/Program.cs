using Serilog;
using Ubs.Monitoring.Api.Extensions;
using Ubs.Monitoring.Api.Startup;
using Ubs.Monitoring.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.AddSerilogLogging(builder.Configuration);

// Application + Infrastructure registrations
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiHealthChecks();


// API cross-cutting concerns
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddApiServices();
builder.Services.AddSwaggerServices();
builder.Services.AddFrontendCors(builder.Configuration);

var app = builder.Build();


app.UseRouting();

// Request logging and error handling
app.UseSerilogRequestLogging();
app.UseApiErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerPipeline();
}

await app.InitializeDatabaseAsync();

app.UseFrontendCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapApiHealthChecks();
app.MapControllers();
app.Run();
