using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using SanitizedHttpLogger;
using SanitizedHttpLogger.Options;
using SanitizedHttpLogger.Services;
using Stef.Validation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseSanitizedHttpLogger(this IServiceCollection services, IConfiguration configuration)
    {
        Guard.NotNull(services);
        Guard.NotNull(configuration);

        return services.UseSanitizedHttpLogger(restEaseClientOptions =>
        {
            configuration.GetSection(nameof(SanitizedHttpLoggerOptions)).Bind(restEaseClientOptions);
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
            .AddSingleton<IRequestUriReplacer, RequestUriReplacer>()
            .Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, ReplaceLoggingHttpMessageHandlerBuilderFilter>());
    }
}