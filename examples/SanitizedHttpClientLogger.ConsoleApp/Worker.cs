using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SanitizedHttpClientLogger.ConsoleApp;

internal class Worker(ILogger<Worker> logger, IHttpClientFactory factory)
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var httpClient = factory.CreateClient(nameof(Worker));
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", "my-secret-key");
        httpClient.DefaultRequestHeaders.Add("X-Api-Key2", ["my-secret-key", "abc"]);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "secret-token");

        var result = await httpClient.GetStringAsync("/abc?apikey=my-secret-key", cancellationToken);
        logger.LogInformation("Result: {Result}", result);

        try
        {
            await httpClient.GetStringAsync("/error?apikey=my-secret-key", cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Oops");
        }
    }
}