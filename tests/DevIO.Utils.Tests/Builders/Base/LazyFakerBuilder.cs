using Bogus;

namespace DevIO.Utils.Tests.Builders.Base;

public abstract class LazyFakerBuilder<TEntity> where TEntity : class, new()
{
    protected const string Locale = "pt_BR";
    private readonly Lazy<Faker<TEntity>> _lazyFaker;

    protected LazyFakerBuilder()
    {
        _lazyFaker = new Lazy<Faker<TEntity>>(Factory, isThreadSafe: true);
    }

    public virtual TEntity Build()
    {
        return Faker.Generate();
    }

    public ICollection<TEntity> BuildCollection(int? count = null)
    {
        count ??= new Faker().Random.Number(5, 15);

        return Enumerable
            .Range(0, count.Value)
            .Select(_ => Build())
            .ToArray();
    }

    protected Faker<TEntity> Faker => _lazyFaker.Value;

    protected abstract Faker<TEntity> Factory();
}