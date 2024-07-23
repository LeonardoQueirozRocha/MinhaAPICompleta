using Bogus;
using DevIO.Business.Configurations;
using DevIO.Utils.Tests.Builders.Base;

namespace DevIO.Utils.Tests.Builders.Business.Configurations;

public class ValidationMessagesBuilder : LazyFakerBuilder<ValidationMessages>
{
    private ValidationMessagesBuilder() { }

    public static ValidationMessagesBuilder Instance => new();

    protected override Faker<ValidationMessages> Factory() => 
        new Faker<ValidationMessages>(Locale)
            .RuleFor(op => op.EmptyFileMessage, setter => setter.Random.Word())
            .RuleFor(op => op.FileAlreadyExistMessage, setter => setter.Random.Word())
            .RuleFor(op => op.IncorrectUserOrPasswordMessage, setter => setter.Random.Word())
            .RuleFor(op => op.LockedOutMessage, setter => setter.Random.Word())
            .RuleFor(op => op.QueryError, setter => setter.Random.Word())
            .RuleFor(op => op.SupplierAlreadyExist, setter => setter.Random.Word())
            .RuleFor(op => op.SupplierHasRegisteredProducts, setter => setter.Random.Word())
            .RuleFor(op => op.SupplierNotFound, setter => setter.Random.Word());
}