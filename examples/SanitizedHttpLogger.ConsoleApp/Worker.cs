using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SanitizedHttpLogger.ConsoleApp;

internal class Worker
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _factory;

    public Worker(ILogger<Worker> logger, IHttpClientFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var httpClient = _factory.CreateClient(nameof(Worker));
        
        var result = await httpClient.GetStringAsync("/abc?apikey=my-secret-key", cancellationToken);
        _logger.LogInformation("Result: {Result}", result);

        try
        {
            await httpClient.GetStringAsync("/error?apikey=my-secret-key", cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Oops");
        }
    }
}