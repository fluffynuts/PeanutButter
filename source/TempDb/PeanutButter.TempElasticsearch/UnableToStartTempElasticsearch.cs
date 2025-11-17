using System;

namespace PeanutButter.TempElasticsearch;

/// <summary>
/// Thrown when TempElasticsearch is unable to verify
/// that elasticsearch has started up by querying the
/// root document
/// </summary>
public class UnableToStartTempElasticsearch : Exception
{
    /// <inheritdoc />
    public UnableToStartTempElasticsearch(
        Exception inner
    ) : base("Unable to verify connection to temporary elasticsearch", inner)
    {
    }

    /// <inheritdoc />
    public UnableToStartTempElasticsearch(
        string message
    )
        : base(message)
    {
    }
}