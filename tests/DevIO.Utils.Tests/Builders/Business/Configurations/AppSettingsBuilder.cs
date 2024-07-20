using DevIO.Business.Configurations;

namespace DevIO.Utils.Tests.Builders.Business.Configurations;

public static class AppSettingsBuilder
{
    public static AppSettings Build()
    {
        var appSettings = new AppSettings
        {
            AuthConfiguration = AuthConfigurationBuilder.Build(),
            LogConfiguration = LogConfigurationBuilder.Build(),
            ValidationMessages = ValidationMessagesBuilder.Build(),
        };

        return appSettings;
    }
}