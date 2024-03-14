using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SanitizedHttpLogger.Services;
using Stef.Validation;

namespace SanitizedHttpLogger.Logging;

internal class SanitizedLogger : DelegatingHandler
{
    private readonly ILogger _logger;
    private readonly IRequestUriReplacer _requestUriReplacer;

    public SanitizedLogger(ILogger logger, IRequestUriReplacer requestUriReplacer)
    {
        _logger = Guard.NotNull(logger);
        _requestUriReplacer = Guard.NotNull(requestUriReplacer);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.NotNull(request);

        var requestUri = request.RequestUri?.ToString(); // SendAsync modifies req uri in case of redirects (?!), so make a local copy
        var sanitizedRequestUri = _requestUriReplacer.Replace(requestUri);

        var stopwatch = ValueStopwatch.StartNew();

        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("{Method} {SanitizedUri} - {StatusCode} in {ElapsedTime}ms", request.Method, sanitizedRequestUri, response.StatusCode, stopwatch.GetElapsedTime().TotalMilliseconds);
            return response;
        }
        catch (Exception)
        {
            _logger.LogInformation("{Method} {SanitizedUri} failed to respond in {ElapsedTime}ms", request.Method, sanitizedRequestUri, stopwatch.GetElapsedTime().TotalMilliseconds);
            throw;
        }
    }

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
}