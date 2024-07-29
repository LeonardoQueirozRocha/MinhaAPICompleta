using Bogus;
using DevIO.Utils.Tests.Builders.Base;
using Microsoft.AspNetCore.Identity;

namespace DevIO.Utils.Tests.Builders.Business.Models.Auth;

public class IdentityErrorBuilder : LazyFakerBuilder<IdentityError>
{
    private IdentityErrorBuilder() { }

    public static IdentityErrorBuilder Instance => new();

    protected override Faker<IdentityError> Factory() =>
        new Faker<IdentityError>(Locale)
            .RuleFor(op => op.Code, setter => setter.Random.Word())
            .RuleFor(op => op.Description, setter => setter.Random.Word());
}