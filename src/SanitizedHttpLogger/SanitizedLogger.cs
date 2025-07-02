using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SanitizedHttpClientLogger.Services;

namespace SanitizedHttpLogger;

internal class SanitizedLogger : DelegatingHandler
{
    private readonly ILogger _logger;
    private readonly IRequestUriReplacer _requestUriReplacer;

    public SanitizedLogger(ILogger logger, IRequestUriReplacer requestUriReplacer)
    {
        _logger = Guard.NotNull(logger);
        _requestUriReplacer = Guard.NotNull(requestUriReplacer);
    }

#if !(NETSTANDARD2_0 || NETSTANDARD2_1 || NET48)
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.NotNull(request);

        var sanitizedRequestUri = SanitizeRequestUri(request);
        var stopwatch = ValueStopwatch.StartNew();

        try
        {
            LogRequestAsInfo(request, sanitizedRequestUri);
            var response = base.Send(request, cancellationToken);
            return LogResponseAsInfo(request, sanitizedRequestUri, response, stopwatch);
        }
        catch (Exception exception)
        {
            LogResponseAsWarning(exception, request, sanitizedRequestUri, stopwatch);
            throw;
        }
    }
#endif

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.NotNull(request);

        var sanitizedRequestUri = SanitizeRequestUri(request);
        var stopwatch = ValueStopwatch.StartNew();

        try
        {
            LogRequestAsInfo(request, sanitizedRequestUri);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return LogResponseAsInfo(request, sanitizedRequestUri, response, stopwatch);
        }
        catch (Exception exception)
        {
            LogResponseAsWarning(exception, request, sanitizedRequestUri, stopwatch);
            throw;
        }
    }

    private Uri? SanitizeRequestUri(HttpRequestMessage request)
    {
        var replaced = _requestUriReplacer.Replace(request.RequestUri?.ToString());
        return replaced == null ? null : new Uri(replaced);
    }

    private void LogRequestAsInfo(HttpRequestMessage request, Uri? sanitizedRequestUri)
    {
        _logger.LogInformation("Sending HTTP request {Method} {SanitizedUri}", request.Method, sanitizedRequestUri);
    }

    private HttpResponseMessage LogResponseAsInfo(HttpRequestMessage request, Uri? sanitizedRequestUri, HttpResponseMessage response, ValueStopwatch stopwatch)
    {
        _logger.LogInformation("Received HTTP response {Method} {SanitizedUri} - {StatusCode} in {ElapsedTime}ms", request.Method, sanitizedRequestUri, (int)response.StatusCode, stopwatch.GetElapsedTime().TotalMilliseconds.ToString("F1"));
        return response;
    }

    private void LogResponseAsWarning(Exception exception, HttpRequestMessage request, Uri? sanitizedRequestUri, ValueStopwatch stopwatch)
    {
        _logger.LogWarning(exception, "HTTP request {Method} {SanitizedUri} failed to respond in {ElapsedTime}ms", request.Method, sanitizedRequestUri, stopwatch.GetElapsedTime().TotalMilliseconds.ToString("F1"));
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