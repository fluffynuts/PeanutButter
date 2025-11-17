using PeanutButter.Utils;

namespace PeanutButter.TempElasticsearch;

/// <summary>
/// Describes a factory for your http server usage:
/// - Take() an IPoolItem&lt;IHttpServer&gt;
/// - work with the server
/// - return it to the pool by disposing of the pool item (use 'using' for safety)
/// </summary>
public interface ITempElasticsearchFactory : IPool<ITempElasticsearch>
{
    /// <summary>
    /// The options to use when providing a new TempElasticsearch
    /// </summary>
    TempElasticSearchOptions Options { get; }
}
/// <inheritdoc cref="PeanutButter.TempElasticsearch.ITempElasticsearch" />
public class TempElasticsearchFactory : Pool<ITempElasticsearch>, ITempElasticsearchFactory
{
    /// <summary>
    /// The options to use when providing a new TempElasticsearch
    /// - since each instance makes a read-only copy of this at
    ///   launch, this is fairly safe to be modified, if you need to,
    ///   otherwise construct with the required options
    /// </summary>
    public TempElasticSearchOptions Options { get; }

    /// <summary>
    /// Constructs a new TempElasticsearchFactory with no limit
    /// on the total number of servers that can be in play
    /// </summary>
    public TempElasticsearchFactory()
        : this(int.MaxValue)
    {
    }

    /// <summary>
    /// Constructs a new TempElasticsearchFactory with the
    /// provided item limit
    /// </summary>
    public TempElasticsearchFactory(int maxItems)
        : this(maxItems, new TempElasticSearchOptions())
    {
    }

    /// <summary>
    /// Constructs a new TempElasticsearchFactory with the
    /// provided item limit and options for creating new
    /// instances of TempElasticsearch
    /// </summary>
    public TempElasticsearchFactory(
        int maxItems,
        TempElasticSearchOptions options
    ) : base(o => Create(o as TempElasticsearchFactory), maxItems)
    {
        Options = options;
    }

    private static ITempElasticsearch Create(
        ITempElasticsearchFactory factory
    )
    {
        return new TempElasticsearch(factory.Options);
    }
}