using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SanitizedHttpClientLogger.Services;

namespace SanitizedHttpLogger;

internal class SanitizedLogger(ILogger logger, IUriReplacer uriReplacer, IHttpHeadersSanitizer headersSanitizer) : DelegatingHandler
{
#if !(NETSTANDARD2_0 || NETSTANDARD2_1 || NET48)
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.NotNull(request);

        var sanitizedRequestUri = SanitizeUri(request);
        var stopwatch = ValueStopwatch.StartNew();

        HttpResponseMessage? response = null;
        try
        {
            LogRequest(request, sanitizedRequestUri);
            response = base.Send(request, cancellationToken);
            return LogResponse(request, sanitizedRequestUri, response, stopwatch);
        }
        catch (Exception exception)
        {
            LogResponseAsWarning(exception, request, sanitizedRequestUri, response, stopwatch);
            throw;
        }
    }
#endif

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.NotNull(request);

        var sanitizedRequestUri = SanitizeUri(request);
        var stopwatch = ValueStopwatch.StartNew();

        HttpResponseMessage? response = null;
        try
        {
            LogRequest(request, sanitizedRequestUri);
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return LogResponse(request, sanitizedRequestUri, response, stopwatch);
        }
        catch (Exception exception)
        {
            LogResponseAsWarning(exception, request, sanitizedRequestUri, response, stopwatch);
            throw;
        }
    }

    private object? SanitizeUri(HttpRequestMessage request)
    {
        return uriReplacer.Replace(request.RequestUri);
    }

    private void LogRequest(HttpRequestMessage request, object? sanitizedRequestUri)
    {
        logger.LogInformation("Sending HTTP request {Method} {SanitizedUri}", request.Method, sanitizedRequestUri);

        if (logger.IsEnabled(LogLevel.Trace))
        {
            var headers = headersSanitizer.Sanitize(request.Headers);
            logger.LogTrace("Request Headers:{Headers}", Environment.NewLine + string.Join(Environment.NewLine, headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));
        }
    }

    private HttpResponseMessage LogResponse(HttpRequestMessage request, object? sanitizedRequestUri, HttpResponseMessage response, ValueStopwatch stopwatch)
    {
        logger.LogInformation("Received HTTP response {Method} {SanitizedUri} - {StatusCode} in {ElapsedTime}ms", request.Method, sanitizedRequestUri, (int)response.StatusCode, stopwatch.GetElapsedTime().TotalMilliseconds.ToString("F1"));

        if (logger.IsEnabled(LogLevel.Trace))
        {
            var headers = headersSanitizer.Sanitize(response.Headers);
            logger.LogTrace("Response Headers:{Headers}", Environment.NewLine + string.Join(Environment.NewLine, headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));
        }

        return response;
    }

    private void LogResponseAsWarning(Exception exception, HttpRequestMessage request, object? sanitizedRequestUri, HttpResponseMessage? response, ValueStopwatch stopwatch)
    {
        logger.LogWarning(exception, "HTTP request {Method} {SanitizedUri} failed to respond in {ElapsedTime}ms", request.Method, sanitizedRequestUri, stopwatch.GetElapsedTime().TotalMilliseconds.ToString("F1"));

        if (logger.IsEnabled(LogLevel.Trace) && response != null)
        {
            var headers = headersSanitizer.Sanitize(response.Headers);
            logger.LogTrace("Response Headers:{Headers}", Environment.NewLine + string.Join(Environment.NewLine, headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")));
        }
    }

    #region ValueStopwatch
    /// <summary>
    /// Copied from https://github.com/dotnet/aspnetcore/blob/main/src/Shared/ValueStopwatch/ValueStopwatch.cs
    /// </summary>
    internal readonly struct ValueStopwatch
    {
#if !NET7_0_OR_GREATER
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
#endif

        private readonly long _startTimestamp;

        public bool IsActive => _startTimestamp != 0;

        private ValueStopwatch(long startTimestamp)
        {
            _startTimestamp = startTimestamp;
        }

        public static ValueStopwatch StartNew() => new(Stopwatch.GetTimestamp());

        public TimeSpan GetElapsedTime()
        {
            // Start timestamp can't be zero in an initialized ValueStopwatch. It would have to be literally the first thing executed when the machine boots to be 0.
            // So it being 0 is a clear indication of default(ValueStopwatch)
            if (!IsActive)
            {
                throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used to get elapsed time.");
            }

            var end = Stopwatch.GetTimestamp();

#if !NET7_0_OR_GREATER
            var timestampDelta = end - _startTimestamp;
            var ticks = (long)(TimestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
#else
            return Stopwatch.GetElapsedTime(_startTimestamp, end);
#endif
        }
    }
    #endregion
}