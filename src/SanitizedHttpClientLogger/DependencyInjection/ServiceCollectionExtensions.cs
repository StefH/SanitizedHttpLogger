using Microsoft.Extensions.Configuration;
using SanitizedHttpClientLogger;
using SanitizedHttpClientLogger.Services;
using SanitizedHttpLogger.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

[PublicAPI]
public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder ConfigureSanitizedLogging(this IHttpClientBuilder builder, IConfiguration configuration)
    {
        Guard.NotNull(builder);
        Guard.NotNull(configuration);

        return builder.ConfigureSanitizedLogging(sanitizedHttpLoggerOptions =>
        {
            configuration.GetSection(nameof(SanitizedHttpLoggerOptions)).Bind(sanitizedHttpLoggerOptions);
        });
    }

    public static IHttpClientBuilder ConfigureSanitizedLogging(this IHttpClientBuilder builder, IConfigurationSection section)
    {
        Guard.NotNull(builder);
        Guard.NotNull(section);

        return builder.ConfigureSanitizedLogging(section.Bind);
    }

    public static IHttpClientBuilder ConfigureSanitizedLogging(this IHttpClientBuilder builder, Action<SanitizedHttpLoggerOptions> configureAction)
    {
        Guard.NotNull(builder);
        Guard.NotNull(configureAction);

        var options = new SanitizedHttpLoggerOptions();
        configureAction(options);

        return builder.ConfigureSanitizedLogging(options);
    }

    public static IHttpClientBuilder ConfigureSanitizedLogging(this IHttpClientBuilder builder, SanitizedHttpLoggerOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.Services
            .AddOptionsWithDataAnnotationValidation(options)
            .AddScoped<SanitizedLogger>()
            .AddSingleton<IRequestUriReplacer, RequestUriReplacer>();

        return builder
            .RemoveAllLoggers()
            .AddLogger<SanitizedLogger>(wrapHandlersPipeline: true);
    }
}