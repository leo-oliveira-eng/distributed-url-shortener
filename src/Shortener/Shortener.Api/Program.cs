var builder = WebApplication.CreateBuilder(args);

var httpPort = GetHttpPort(builder.Configuration, "SHORTENER_API_HTTP_PORT", 5101);
builder.WebHost.UseUrls($"http://0.0.0.0:{httpPort}");

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

static int GetHttpPort(IConfiguration configuration, string configurationKey, int defaultPort)
{
    var configuredPort = configuration[configurationKey];

    if (string.IsNullOrWhiteSpace(configuredPort))
    {
        return defaultPort;
    }

    if (!int.TryParse(configuredPort, out var httpPort) || httpPort is < 1 or > 65535)
    {
        throw new InvalidOperationException(
            $"Configuration value '{configurationKey}' must be a valid TCP port.");
    }

    return httpPort;
}
