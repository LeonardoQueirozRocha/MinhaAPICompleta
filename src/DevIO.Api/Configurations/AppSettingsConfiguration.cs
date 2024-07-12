using DevIO.Business.Configurations;
using Microsoft.Extensions.Options;

namespace DevIO.Api.Configurations;

public static class AppSettingsConfiguration
{
    public static AppSettings AddAppSettingsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        services
            .AddConfiguration<AppSettings>(configuration)
            .AddConfiguration<AuthConfiguration>(configuration, nameof(AuthConfiguration))
            .AddConfiguration<LogConfiguration>(configuration, nameof(LogConfiguration));

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<AppSettings>();
    }

    private static IServiceCollection AddConfiguration<TParameterType>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = null) where TParameterType : class, new()
    {
        var section = string.IsNullOrEmpty(sectionName) ? configuration : configuration.GetSection(sectionName);

        services
            .Configure<TParameterType>(section)
            .AddScoped(provider => provider.GetService<IOptionsSnapshot<TParameterType>>().Value);

        return services;
    }
}