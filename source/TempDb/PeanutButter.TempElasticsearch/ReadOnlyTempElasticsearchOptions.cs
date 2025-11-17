using System;

namespace PeanutButter.TempElasticsearch;

/// <summary>
/// Read-only TempElasticsearch options
/// </summary>
public class ReadOnlyTempElasticsearchOptions
{
    /// <summary>
    /// The port this instance is configured to use
    /// </summary>
    public int Port { get; private set; }

    /// <summary>
    /// The user this instance is configured to use for auth
    /// </summary>
    public string User { get; private set; }

    /// <summary>
    /// The password this instance is configured to use for auth
    /// </summary>
    public string Password { get; private set; }

    /// <summary>
    /// The number of cpus that docker is allowed to use
    /// </summary>
    public int CPUs { get; private set; }

    /// <summary>
    /// The max memory docker is allowed to use
    /// </summary>
    public int MaxMemoryMb { get; private set; }

    /// <summary>
    /// The version of the elasticsearch docker
    /// image being used
    /// </summary>
    public string Version { get; private set; }

    /// <summary>
    /// The maximum amount of time, in seconds, to
    /// allow for the dockerised elasticsearch to
    /// become available before giving up.
    /// When a connection cannot be established within this time,
    /// an UnableToStartTempElasticsearch exception is raised
    /// </summary>
    public int MaxTimeAllowedToComeUpInSeconds { get; set; } = 30;

    internal static ReadOnlyTempElasticsearchOptions From(
        TempElasticSearchOptions options
    )
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return new()
        {
            Port = options.Port,
            User = options.User,
            Password = options.Password,
            CPUs = options.CPUs,
            MaxMemoryMb = options.MaxMemoryMb,
            Version = options.Version,
            MaxTimeAllowedToComeUpInSeconds = options.MaxTimeAllowedToComeUpInSeconds
        };
    }
}