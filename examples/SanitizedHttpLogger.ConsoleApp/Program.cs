using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SanitizedHttpClientLogger.ConsoleApp;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SanitizedHttpLogger.ConsoleApp;

static class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateLogger();

        await using var serviceProvider = RegisterServices(args);

        var worker = serviceProvider.GetRequiredService<Worker>();

        await worker.RunAsync(CancellationToken.None);
    }

    private static ServiceProvider RegisterServices(string[] args)
    {
        var configuration = SetupConfiguration(args);
        var services = new ServiceCollection();

        services.AddSingleton(configuration);
        
        services.AddLogging(builder => builder.AddSerilog(logger: Log.Logger, dispose: true));

        var mockServer = WireMockServer.Start();
        mockServer
            .Given(Request.Create().WithPath("/abc")
                .UsingGet()
            )
            .RespondWith(Response.Create()
                .WithBody("Test")
                .WithStatusCode(HttpStatusCode.OK)
            );
        mockServer
            .Given(Request.Create().WithPath("/error")
                .UsingGet()
            )
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.InternalServerError)
            );

        services.AddHttpClient<Worker>((_, o) =>
        {
            o.BaseAddress = new Uri(mockServer.Urls[0]);
            // o.BaseAddress = new Uri("https://_");
        });

        services.UseSanitizedHttpLogger(configuration);

        services.AddSingleton<Worker>();

        return services.BuildServiceProvider();
    }

    private static IConfiguration SetupConfiguration(string[] args)
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
    }
}