using Bogus;
using DevIO.Business.Configurations;
using DevIO.Utils.Tests.Builders.Base;

namespace DevIO.Utils.Tests.Builders.Business.Configurations;

public class LogConfigurationBuilder : LazyFakerBuilder<LogConfiguration>
{
    private LogConfigurationBuilder() { }

    public static LogConfigurationBuilder Instance => new();

    protected override Faker<LogConfiguration> Factory() =>
        new Faker<LogConfiguration>(Locale)
            .RuleFor(op => op.ApiKey, _ => Guid.NewGuid().ToString())
            .RuleFor(op => op.LogId, _ => Guid.NewGuid())
            .RuleFor(op => op.HeartbeatId, _ => Guid.NewGuid().ToString());
}
