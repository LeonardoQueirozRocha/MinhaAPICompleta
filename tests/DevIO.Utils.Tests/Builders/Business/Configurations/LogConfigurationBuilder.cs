using DevIO.Business.Configurations;

namespace DevIO.Utils.Tests.Builders.Business.Configurations;

public static class LogConfigurationBuilder
{
    public static LogConfiguration Build()
    {
        var logConfiguration = new LogConfiguration
        {
            ApiKey = Guid.NewGuid().ToString(),
            LogId = Guid.NewGuid(),
            HeartbeatId = Guid.NewGuid().ToString()
        };

        return logConfiguration;
    }
}
