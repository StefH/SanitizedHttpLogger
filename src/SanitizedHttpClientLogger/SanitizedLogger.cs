using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using SanitizedHttpClientLogger.Services;

namespace SanitizedHttpClientLogger;

internal class SanitizedLogger(ILogger<SanitizedLogger> logger, IUriReplacer uriReplacer, IHttpHeadersSanitizer headersReplacer) : IHttpClientLogger
{
    public object? LogRequestStart(HttpRequestMessage request)
    {
        Guard.NotNull(request);

        logger.LogInformation("Sending HTTP request {Method} {SanitizedUri}", request.Method, SanitizeUri(request));

        if (logger.IsEnabled(LogLevel.Trace))
        {
            var headers = headersReplacer.Sanitize(request.Headers);
            logger.LogTrace("Request Headers:{Headers}", Environment.NewLine + string.Join(Environment.NewLine, headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));
        }

        return null;
    }

    public void LogRequestStop(object? context, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
    {
        Guard.NotNull(request);
        Guard.NotNull(response);

        logger.LogInformation("Received HTTP response {Method} {SanitizedUri} - {StatusCode} in {ElapsedTime}ms", request.Method, SanitizeUri(request), (int)response.StatusCode, elapsed.TotalMilliseconds.ToString("F1"));

        if (logger.IsEnabled(LogLevel.Trace))
        {
            var headers = headersReplacer.Sanitize(response.Headers);
            logger.LogTrace("Response Headers:{Headers}", Environment.NewLine + string.Join(Environment.NewLine, headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));
        }
    }

    public void LogRequestFailed(object? context, HttpRequestMessage request, HttpResponseMessage? response, Exception exception, TimeSpan elapsed)
    {
        Guard.NotNull(request);

        logger.LogWarning(exception, "HTTP request {Method} {SanitizedUri} failed to respond in {ElapsedTime}ms", request.Method, SanitizeUri(request), elapsed.TotalMilliseconds.ToString("F1"));

        if (logger.IsEnabled(LogLevel.Trace) && response != null)
        {
            var headers = headersReplacer.Sanitize(response.Headers);
            logger.LogTrace("Response Headers:{Headers}", Environment.NewLine + string.Join(Environment.NewLine, headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));
        }
    }

    private object? SanitizeUri(HttpRequestMessage request)
    {
        return uriReplacer.Replace(request.RequestUri);
    }
}