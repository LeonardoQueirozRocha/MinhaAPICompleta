using Bogus;
using DevIO.Business.Configurations;
using DevIO.Utils.Tests.Builders.Base;

namespace DevIO.Utils.Tests.Builders.Business.Configurations;

public class AppSettingsBuilder : LazyFakerBuilder<AppSettings>
{
    private AppSettingsBuilder() { }

    public static AppSettingsBuilder Instance => new();

    protected override Faker<AppSettings> Factory() =>
        new Faker<AppSettings>(Locale)
            .RuleFor(op => op.AuthConfiguration, _ => AuthConfigurationBuilder.Instance.Build())
            .RuleFor(op => op.LogConfiguration, _ => LogConfigurationBuilder.Instance.Build())
            .RuleFor(op => op.ValidationMessages, _ => ValidationMessagesBuilder.Instance.Build());
}