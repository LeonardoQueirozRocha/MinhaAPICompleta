using DevIO.Business.Configurations;

namespace DevIO.Utils.Tests.Builders.Business.Configurations;

public static class AuthConfigurationBuilder
{
    public static AuthConfiguration Build()
    {
        var authConfiguration = new AuthConfiguration
        {
            ExpirationHours = 2,
            Issuer = "https://localhost",
            Secret = "MEUSEGREDOSUPERSECRETO",
            ValidIn = "MeuSistema"
        };

        return authConfiguration;
    }
}
