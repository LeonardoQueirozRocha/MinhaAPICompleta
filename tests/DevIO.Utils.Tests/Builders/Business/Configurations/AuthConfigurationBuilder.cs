using Bogus;
using DevIO.Business.Configurations;
using DevIO.Utils.Tests.Builders.Base;

namespace DevIO.Utils.Tests.Builders.Business.Configurations;

public class AuthConfigurationBuilder : LazyFakerBuilder<AuthConfiguration>
{
    private AuthConfigurationBuilder() { }

    public static AuthConfigurationBuilder Instance => new();

    protected override Faker<AuthConfiguration> Factory() =>
         new Faker<AuthConfiguration>(Locale)
            .RuleFor(op => op.ExpirationHours, setter => setter.Random.Int(1, 3))
            .RuleFor(op => op.Issuer, setter => setter.Internet.DomainName())
            .RuleFor(op => op.Secret, setter => setter.Random.AlphaNumeric(30))
            .RuleFor(op => op.ValidIn, setter => setter.Internet.Url());
}
