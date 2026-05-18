var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var applicationEnvironment = builder.Configuration["APP_ENVIRONMENT"]
    ?? builder.Environment.EnvironmentName;

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Shortener.Api",
    environment = applicationEnvironment
}));

app.Run();

