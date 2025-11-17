using System;
using System.Net;
using PeanutButter.Utils;

namespace PeanutButter.TempElasticsearch;

/// <summary>
/// 
/// </summary>
public class TempElasticSearchOptions
{
    /// <summary>
    /// The desired port to listen on
    /// - defaults to 9200
    /// </summary>
    public int Port { get; set; } = PortFinder.FindOpenPort(IPAddress.Loopback, 9200, 9300);

    /// <summary>
    /// The desired username for auth
    /// - defaults to empty (no auth)
    /// </summary>
    public string User { get; set; }

    /// <summary>
    /// The desired password for auth
    /// - defaults to empty
    /// - when no username specified, is ignored
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// The max number of cpus to let docker
    /// take over for this
    /// </summary>
    public int CPUs { get; set; } = 2;

    /// <summary>
    /// The max memory, in megabytes, to allow
    /// for the docker container
    /// </summary>
    public int MaxMemoryMb { get; set; } = 2048;

    /// <summary>
    /// Sets the version of the elasticsearch docker
    /// image to start up
    /// </summary>
    public string Version { get; set; } = "9.2.1";

    /// <summary>
    /// The maximum amount of time, in seconds, to
    /// allow for the dockerised elasticsearch to
    /// become available before giving up.
    /// When a connection cannot be established within this time,
    /// an UnableToStartTempElasticsearch exception is raised
    /// </summary>
    public int MaxTimeAllowedToComeUpInSeconds { get; set; } = 45;

    /// <summary>
    /// Normally, docker output is discarded (there is a lot!)
    /// but if you want to debug the docker startup process,
    /// you may set an handler here to collect logs
    /// </summary>
    public Action<string> DockerLogReceiver { get; set; }
}