using Serilog;
using Tawqi3i.QrSanad.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

var app = builder.Build();

app.ConfigurePipeline();

try
{
    await app.Bootstrap();
    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>
/// Exposed for WebApplicationFactory in integration tests.
/// </summary>
public partial class Program
{
}
