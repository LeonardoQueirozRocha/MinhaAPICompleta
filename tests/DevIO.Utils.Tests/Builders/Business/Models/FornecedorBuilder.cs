using Bogus;
using Bogus.Extensions.Brazil;
using DevIO.Business.Models;
using DevIO.Business.Models.Enums;
using DevIO.Utils.Tests.Builders.Base;

namespace DevIO.Utils.Tests.Builders.Business.Models;

public class FornecedorBuilder : LazyFakerBuilder<Fornecedor>
{
    private FornecedorBuilder() { }

    public static FornecedorBuilder Instance => new();

    protected override Faker<Fornecedor> Factory() =>
        new Faker<Fornecedor>(Locale)
            .RuleFor(op => op.Nome, setter => setter.Name.FindName())
            .RuleFor(op => op.Documento, setter => setter.Person.Cpf(false))
            .RuleFor(op => op.TipoFornecedor, setter => setter.PickRandom(
                TipoFornecedor.PessoaFisica,
                TipoFornecedor.PessoaJuridica))
            .RuleFor(op => op.Ativo, setter => setter.Random.Bool());
}