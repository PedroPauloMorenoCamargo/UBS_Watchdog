using Serilog;
using Ubs.Monitoring.Api.Extensions;
using Ubs.Monitoring.Api.Hubs;
using Ubs.Monitoring.Api.Notifications;
using Ubs.Monitoring.Application.Cases.Notifications;
using Ubs.Monitoring.Infrastructure;
using Ubs.Monitoring.Api.Startup;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.AddSerilogLogging(builder.Configuration);

// SignalR
builder.Services.AddSignalR();

// HTTP context
builder.Services.AddHttpContextAccessor();

// Infrastructure + Application
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiHealthChecks();

// Security
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// API layer
builder.Services.AddApiServices();
builder.Services.AddSwaggerServices();
builder.Services.AddFrontendCors(builder.Configuration);

// 🔴 REGISTER publisher HERE (API only)
builder.Services.AddScoped<
    ICaseNotificationPublisher,
    SignalRCaseNotificationPublisher
>();

var app = builder.Build();

app.UseRouting();
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

// 🔴 THIS WAS MISSING (caused SignalR 404)
app.MapHub<CaseNotificationsHub>("/hubs/cases");

app.Run();
