using Bogus.Extensions.Brazil;
using Bogus;
using DevIO.Business.Models;
using DevIO.Business.Models.Enums;
using DevIO.Utils.Tests.Builders.Base;

namespace DevIO.Utils.Tests.Builders.Business.Models;

public class FornecedorBuilder : LazyFakerBuilder<Fornecedor>
{
    private FornecedorBuilder()
    {
    }

    public static FornecedorBuilder Instance => new();

    protected override Faker<Fornecedor> Factory() =>
        new Faker<Fornecedor>(Locale)
            .RuleFor(op => op.Id, setter => setter.Random.Guid())
            .RuleFor(op => op.Nome, setter => setter.Name.FindName())
            .RuleFor(
                op => op.TipoFornecedor,
                setter => setter.PickRandom(TipoFornecedor.PessoaFisica, TipoFornecedor.PessoaJuridica))
            .RuleFor(
                op => op.Documento,
                (setter, current)
                    => current.TipoFornecedor is TipoFornecedor.PessoaFisica
                        ? setter.Person.Cpf(includeFormatSymbols: false)
                        : setter.Company.Cnpj(includeFormatSymbols: false))
            .RuleFor(op => op.Ativo, setter => setter.Random.Bool())
            .RuleFor(op => op.Endereco, _ => EnderecoBuilder.Instance.Build())
            .RuleFor(op => op.Produtos, _ => ProdutoBuilder.Instance.BuildCollection(1));
}