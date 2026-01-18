using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using SanitizedHttpClientLogger.Services;

namespace SanitizedHttpLogger;

internal class ReplaceLoggingHttpMessageHandlerBuilderFilter(ILoggerFactory loggerFactory, IUriReplacer uriReplacer, IHttpHeadersReplacer headersReplacer) : IHttpMessageHandlerBuilderFilter
{
    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            next(builder);

            var loggerName = !string.IsNullOrEmpty(builder.Name) ? builder.Name : "Default";
            var innerLogger = loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.ClientHandler");
            var handlersToRemove = builder.AdditionalHandlers
                .Where(delegatingHandler => delegatingHandler is LoggingHttpMessageHandler or LoggingScopeHttpMessageHandler)
                .ToArray();
            foreach (var delegatingHandler in handlersToRemove)
            {
                builder.AdditionalHandlers.Remove(delegatingHandler);
            }

            builder.AdditionalHandlers.Add(new SanitizedLogger(innerLogger, uriReplacer, headersReplacer));
        };
    }
}