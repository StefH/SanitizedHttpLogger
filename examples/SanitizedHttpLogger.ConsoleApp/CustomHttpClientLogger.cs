using System;
using System.Net.Http;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using Stef.Validation;

namespace SanitizedHttpLogger.ConsoleApp;

public class CustomHttpClientLogger : IHttpClientLogger
{
    private readonly ILogger<CustomHttpClientLogger> _logger;

    public CustomHttpClientLogger(ILogger<CustomHttpClientLogger> logger)
    {
        _logger = Guard.NotNull(logger);
    }

    public object? LogRequestStart(HttpRequestMessage request)
    {
        // Call _logger.LogInformation to log the request start
        // Make sure to sanitize the RequestUri
        return null;
    }

    public void LogRequestStop(object? context, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
    {
        // Call _logger.LogInformation to log the request stop
        // Make sure to sanitize the RequestUri
    }

    public void LogRequestFailed(object? context, HttpRequestMessage request, HttpResponseMessage? response, Exception exception, TimeSpan elapsed)
    {
        // Call _logger.Error to log the error
        // Make sure to sanitize the RequestUri
    }
}