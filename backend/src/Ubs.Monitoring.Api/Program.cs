using Serilog;
using Ubs.Monitoring.Infrastructure;
using Ubs.Monitoring.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RFC7807 / problem+json
builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration);

// CORS (dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();   
app.UseStatusCodePages();   


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await ApplyMigrationsWithRetryAsync(app);
    await SeedDatabaseAsync(app);
}

static async Task ApplyMigrationsWithRetryAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    const int maxRetries = 10;
    const int delayMs = 1500;

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            db.Database.Migrate();
            return;
        }
        catch when (attempt < maxRetries)
        {
            await Task.Delay(delayMs);
        }
    }

    throw new Exception("Database migrations failed after multiple retries.");
}

static async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var seeder = new DatabaseSeeder(db);
    await seeder.SeedAsync();
}


app.UseCors("frontend");

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health/db", async (AppDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok(new { db = "up" })
        : Results.Problem("Cannot connect to database");
});


app.Run();
