using Bogus;
using DevIO.Utils.Tests.Builders.Base;
using Microsoft.AspNetCore.Identity;

namespace DevIO.Utils.Tests.Builders.Business.Models.Auth;

public class IdentityUserBuilder : LazyFakerBuilder<IdentityUser>
{
    private IdentityUserBuilder() { }

    public static IdentityUserBuilder Instance => new();

    protected override Faker<IdentityUser> Factory() =>
        new Faker<IdentityUser>(Locale)
            .RuleFor(op => op.Email, setter => setter.Internet.Email())
            .RuleFor(op => op.Id, _ => Guid.NewGuid().ToString());
}