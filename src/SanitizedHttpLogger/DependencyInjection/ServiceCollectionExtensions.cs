using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using SanitizedHttpClientLogger.Options;
using SanitizedHttpClientLogger.Services;
using SanitizedHttpLogger;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseSanitizedHttpLogger(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.NotNull(services);
        Guard.NotNull(configuration);

        return services.UseSanitizedHttpLogger(sanitizedHttpLoggerOptions =>
        {
            configuration.GetSection(nameof(SanitizedHttpLoggerOptions)).Bind(sanitizedHttpLoggerOptions);
        });
    }

    public static IServiceCollection UseSanitizedHttpLogger(this IServiceCollection services, IConfigurationSection section)
    {
        Guard.NotNull(services);
        Guard.NotNull(section);

        return services.UseSanitizedHttpLogger(section.Bind);
    }

    public static IServiceCollection UseSanitizedHttpLogger(this IServiceCollection services, Action<SanitizedHttpLoggerOptions> configureAction)
    {
        Guard.NotNull(services);
        Guard.NotNull(configureAction);

        var options = new SanitizedHttpLoggerOptions();
        configureAction(options);

        return services.UseSanitizedHttpLogger(options);
    }

    public static IServiceCollection UseSanitizedHttpLogger(this IServiceCollection services, SanitizedHttpLoggerOptions options)
    {
        Guard.NotNull(services);
        Guard.NotNull(options);

        return services
            .AddOptionsWithDataAnnotationValidation(options)
            .AddSingleton<IUriReplacer, UriReplacer>()
            .AddSingleton<IHttpHeadersSanitizer, HttpHeadersSanitizer>()
            .Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, ReplaceLoggingHttpMessageHandlerBuilderFilter>());
    }
}