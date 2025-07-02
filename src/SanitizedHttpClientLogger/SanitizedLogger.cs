using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using SanitizedHttpClientLogger.Services;

namespace SanitizedHttpClientLogger;

internal class SanitizedLogger : IHttpClientLogger
{
    private readonly ILogger<SanitizedLogger> _logger;
    private readonly IRequestUriReplacer _requestUriReplacer;

    public SanitizedLogger(ILogger<SanitizedLogger> logger, IRequestUriReplacer requestUriReplacer)
    {
        _logger = Guard.NotNull(logger);
        _requestUriReplacer = Guard.NotNull(requestUriReplacer);
    }

    public object? LogRequestStart(HttpRequestMessage request)
    {
        Guard.NotNull(request);

        _logger.LogInformation("Sending HTTP request {Method} {SanitizedUri}", request.Method, SanitizeRequestUri(request));

        return null;
    }

    public void LogRequestStop(object? context, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
    {
        Guard.NotNull(request);
        Guard.NotNull(response);

        _logger.LogInformation("Received HTTP response {Method} {SanitizedUri} - {StatusCode} in {ElapsedTime}ms", request.Method, SanitizeRequestUri(request), (int)response.StatusCode, elapsed.TotalMilliseconds.ToString("F1"));
    }

    public void LogRequestFailed(object? context, HttpRequestMessage request, HttpResponseMessage? response, Exception exception, TimeSpan elapsed)
    {
        Guard.NotNull(request);

        _logger.LogWarning(exception, "HTTP request {Method} {SanitizedUri} failed to respond in {ElapsedTime}ms", request.Method, SanitizeRequestUri(request), elapsed.TotalMilliseconds.ToString("F1"));
    }

    private object? SanitizeRequestUri(HttpRequestMessage request)
    {
        return _requestUriReplacer.Replace(request.RequestUri);
    }
}