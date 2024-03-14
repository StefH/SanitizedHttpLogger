using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using SanitizedHttpLogger.Services;
using Stef.Validation;

namespace SanitizedHttpLogger.Logging;

internal class ReplaceLoggingHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IRequestUriReplacer _requestUriReplacer;

    public ReplaceLoggingHttpMessageHandlerBuilderFilter(ILoggerFactory loggerFactory, IRequestUriReplacer requestUriReplacer)
    {
        _loggerFactory = Guard.NotNull(loggerFactory);
        _requestUriReplacer = Guard.NotNull(requestUriReplacer);
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            next(builder);

            var loggerName = !string.IsNullOrEmpty(builder.Name) ? builder.Name : "Default";
            var innerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.ClientHandler");
            var handlersToRemove = builder.AdditionalHandlers.Where(delegatingHandler => delegatingHandler is LoggingHttpMessageHandler or LoggingScopeHttpMessageHandler);
            foreach (var delegatingHandler in handlersToRemove)
            {
                builder.AdditionalHandlers.Remove(delegatingHandler);
            }

            builder.AdditionalHandlers.Add(new SanitizedLogger(innerLogger, _requestUriReplacer));
        };
    }
}