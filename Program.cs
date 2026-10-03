var builder = WebApplication.CreateBuilder(args);

// 1) PORT: listen on the port the hosting platform gives us (Render/Railway set PORT).
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 2) Configuration comes from environment variables (never from code):
//    ConnectionStrings__Default, Cors__AllowedOrigins, Jwt__Key, Jwt__Issuer, Jwt__Audience
var rawConnection = builder.Configuration.GetConnectionString("Default");
var connectionString = string.IsNullOrWhiteSpace(rawConnection)
    ? null
    : DbHelper.Normalize(rawConnection);

// 3) CORS: allow the deployed frontend's exact origin (https, no trailing slash).
var origins = (builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(o => o.TrimEnd('/'))
    .ToArray();

builder.Services.AddCors(o => o.AddPolicy("Frontend", p =>
    p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
var log = app.Logger;
var dbReady = false;

log.LogInformation("CampusEats API starting in {Environment} mode on port {Port}",
    app.Environment.EnvironmentName, port);
log.LogInformation("Allowed CORS origins: {Origins}", string.Join(", ", origins));

if (connectionString is null)
{
    log.LogWarning("ConnectionStrings__Default is not set - using the in-memory menu");
}
else
{
    try
    {
        await DbHelper.EnsureSchemaAsync(connectionString);
        dbReady = true;
        log.LogInformation("Connected to PostgreSQL; menu_items table is ready");
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Database initialisation failed");
    }
}

app.UseCors("Frontend"); // before the endpoints

app.MapGet("/", () => Results.Ok(new { name = "CampusEats API", status = "running" }));

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/api/status", () => Results.Ok(new
{
    environment = app.Environment.EnvironmentName,
    database = connectionString is null ? "not configured" : (dbReady ? "connected" : "error"),
    jwtConfigured = !string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Key"]), // never expose the value
    serverTime = DateTime.UtcNow,
}));

app.MapGet("/api/menu", async () =>
{
    if (connectionString is null || !dbReady)
    {
        log.LogInformation("GET /api/menu - returned {Count} in-memory items", DbHelper.DefaultMenu.Count);
        return Results.Ok(DbHelper.DefaultMenu);
    }

    try
    {
        var items = await DbHelper.GetMenuAsync(connectionString);
        log.LogInformation("GET /api/menu - returned {Count} items from PostgreSQL", items.Count);
        return Results.Ok(items);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "GET /api/menu - database query failed");
        return Results.Problem("Database error", statusCode: 500);
    }
});

app.Run();
