using Elmah.Io.Extensions.Logging;

namespace DevIO.Api.Configurations
{
    public static class LoggerConfig
    {
        public static IServiceCollection AddLoggingConfig(this IServiceCollection services)
        {
            services.AddElmahIo(o =>
            {
                o.ApiKey = "c89e67085c3147b197505b0d6791bbb8";
                o.LogId = new Guid("667e03fa-db1f-49e6-b14c-aa67a10f1512");
            });

            //services.AddLogging(builder =>
            //{
            //    builder.AddElmahIo(configure =>
            //    {
            //        configure.ApiKey = "c89e67085c3147b197505b0d6791bbb8";
            //        configure.LogId = new Guid("667e03fa-db1f-49e6-b14c-aa67a10f1512");
            //    });

            //    builder.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
            //});

            return services;
        }

        public static IApplicationBuilder UseLoggingConfig(this IApplicationBuilder app)
        {
            app.UseElmahIo();

            return app;
        }
    }
}
