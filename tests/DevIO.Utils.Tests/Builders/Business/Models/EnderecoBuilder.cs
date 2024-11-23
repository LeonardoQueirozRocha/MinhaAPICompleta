using Bogus;
using DevIO.Business.Models;
using DevIO.Utils.Tests.Builders.Base;

namespace DevIO.Utils.Tests.Builders.Business.Models;

public class EnderecoBuilder : LazyFakerBuilder<Endereco>
{
    private EnderecoBuilder()
    {
    }

    public static EnderecoBuilder Instance => new();

    protected override Faker<Endereco> Factory() =>
        new Faker<Endereco>(Locale)
            .RuleFor(op => op.Id, setter => setter.Random.Guid())
            .RuleFor(op => op.FornecedorId, setter => setter.Random.Guid())
            .RuleFor(op => op.Logradouro, setter => setter.Address.StreetName())
            .RuleFor(op => op.Numero, setter => setter.Random.Int(1, 999).ToString())
            .RuleFor(op => op.Complemento, setter => setter.Address.Direction())
            .RuleFor(op => op.Cep, setter => setter.Address.ZipCode("########"))
            .RuleFor(op => op.Bairro, setter => setter.Address.StreetAddress())
            .RuleFor(op => op.Cidade, setter => setter.Address.City())
            .RuleFor(op => op.Estado, setter => setter.Address.State());
}