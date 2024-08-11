using Bogus;
using DevIO.Business.Models;
using DevIO.Utils.Tests.Builders.Base;

namespace DevIO.Utils.Tests.Builders.Business.Models;

public class ProdutoBuilder : LazyFakerBuilder<Produto>
{
    private ProdutoBuilder() { }

    public static ProdutoBuilder Instance => new();

    protected override Faker<Produto> Factory() =>
        new Faker<Produto>(Locale)
            .RuleFor(op => op.FornecedorId, setter => setter.Random.Guid())
            .RuleFor(op => op.Nome, setter => setter.Commerce.Product())
            .RuleFor(op => op.Descricao, setter => setter.Commerce.ProductDescription())
            .RuleFor(op => op.Imagem, setter => setter.Lorem.Letter())
            .RuleFor(op => op.Valor, setter => setter.Random.Decimal())
            .RuleFor(op => op.DataCadastro, setter => setter.Date.Recent())
            .RuleFor(op => op.Ativo, setter => setter.Random.Bool());
}