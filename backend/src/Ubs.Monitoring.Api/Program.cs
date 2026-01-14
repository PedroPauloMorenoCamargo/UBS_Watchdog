using Serilog;
using Ubs.Monitoring.Api.Extensions;
using Ubs.Monitoring.Api.Hubs;
using Ubs.Monitoring.Api.Notifications;
using Ubs.Monitoring.Application.Cases.Notifications;
using Ubs.Monitoring.Infrastructure;
using Ubs.Monitoring.Api.Startup;

var builder = WebApplication.CreateBuilder(args);

// Structured logging with Serilog
builder.Host.AddSerilogLogging(builder.Configuration);

// SignalR services for real-time communication
builder.Services.AddSignalR();

// HTTP context accessor to allow access to request context in services
builder.Services.AddHttpContextAccessor();

// Infrastructure and application-layer dependencies
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiHealthChecks();

// Authentication and authorization using JWT
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// API-layer services, Swagger, and CORS configuration
builder.Services.AddApiServices();
builder.Services.AddSwaggerServices();
builder.Services.AddFrontendCors(builder.Configuration);

// Case notification publisher 
builder.Services.AddScoped<ICaseNotificationPublisher, SignalRCaseNotificationPublisher>();

var app = builder.Build();

// HTTP request pipeline
app.UseRouting();
app.UseSerilogRequestLogging();
app.UseApiErrorHandling();

// Enable Swagger only in development environments
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerPipeline();
}

// Apply database migrations and initialization logic at startup
await app.InitializeDatabaseAsync();

// Enable CORS, authentication, and authorization middleware
app.UseFrontendCors();
app.UseAuthentication();
app.UseAuthorization();

// Map health check endpoints and API controllers
app.MapApiHealthChecks();
app.MapControllers();

// Map the SignalR hub endpoint used for case notifications
app.MapHub<CaseNotificationsHub>("/hubs/cases").RequireCors("frontend");

app.Run();
