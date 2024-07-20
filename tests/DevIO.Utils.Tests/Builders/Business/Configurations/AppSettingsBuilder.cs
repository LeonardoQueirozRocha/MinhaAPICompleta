using DevIO.Business.Configurations;

namespace DevIO.Utils.Tests.Builders.Business.Configurations;

public static class AppSettingsBuilder
{
    public static AppSettings Build()
    {
        var appSettings = new AppSettings
        {
            AuthConfiguration = new AuthConfiguration
            {
                ExpirationHours = 2,
                Issuer = "https://localhost",
                Secret = "MEUSEGREDOSUPERSECRETO",
                ValidIn = "MeuSistema"
            },
            LogConfiguration = new LogConfiguration
            {
                ApiKey = Guid.NewGuid().ToString(),
                LogId = Guid.NewGuid(),
                HeartbeatId = Guid.NewGuid().ToString()
            },
            ValidationMessages = new ValidationMessages
            {
                EmptyFileMessage = "EmptyFileMessage",
                FileAlreadyExistMessage = "FileAlreadyExistMessage",
                IncorrectUserOrPasswordMessage = "IncorrectUserOrPasswordMessage",
                LockedOutMessage = "LockedOutMessage",
                QueryError = "QueryError",
                SupplierAlreadyExist = "SupplierAlreadyExist",
                SupplierHasRegisteredProducts = "SupplierHasRegisteredProducts",
                SupplierNotFound = "SupplierNotFound",
            }
        };

        return appSettings;
    }
}