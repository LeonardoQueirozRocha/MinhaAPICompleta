using DevIO.Api.Extensions;
using DevIO.Business.Configurations;
using Elmah.Io.Extensions.Logging;

namespace DevIO.Api.Configurations;

public static class LoggerConfiguration
{
    public static IServiceCollection AddLoggingConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        LogConfiguration logConfiguration)
    {
        services.AddElmahIo(o =>
        {
            o.ApiKey = logConfiguration.ApiKey;
            o.LogId = logConfiguration.LogId;
        });

        services
            .AddHealthChecks()
            .AddElmahIoPublisher(options =>
            {
                options.ApiKey = logConfiguration.ApiKey;
                options.LogId = logConfiguration.LogId;
                options.HeartbeatId = logConfiguration.HeartbeatId;
            })
            .AddCheck("Produtos", new SqlServerHealthCheck(configuration.GetConnectionString("DefaultConnection")))
            .AddSqlServer(configuration.GetConnectionString("DefaultConnection"), name: "BancoSQL");

        services
            .AddHealthChecksUI()
            .AddSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));

        return services;
    }

    public static IApplicationBuilder UseLoggingConfiguration(this IApplicationBuilder app)
    {
        app.UseElmahIo();

        return app;
    }
}
