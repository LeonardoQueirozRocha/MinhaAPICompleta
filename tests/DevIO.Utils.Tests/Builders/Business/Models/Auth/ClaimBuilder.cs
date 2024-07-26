using System.Security.Claims;
using Bogus;
using DevIO.Utils.Tests.Builders.Base;

namespace DevIO.Utils.Tests.Builders.Business.Models.Auth;

public class ClaimBuilder : LazyFakerBuilder<Claim>
{
    private ClaimBuilder() { }

    public static ClaimBuilder Instance => new();

    protected override Faker<Claim> Factory() =>
        new Faker<Claim>(Locale)
            .CustomInstantiator(setter => new Claim(setter.Lorem.Word(), setter.Lorem.Word()));
}