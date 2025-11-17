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
    /// - default: is selected randomly from available open ports
    ///   between 9200 and 9300 inclusive
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
    /// take over for this (default: 2)
    /// </summary>
    public int CPUs { get; set; } = 2;

    /// <summary>
    /// The max memory, in megabytes, to allow
    /// for the docker container (default: 1024)
    /// </summary>
    public int MaxMemoryMb { get; set; } = 1024;

    /// <summary>
    /// Sets the version of the elasticsearch docker
    /// image to start up (default: 9.2.1)
    /// </summary>
    public string Version { get; set; } = "9.2.1";

    /// <summary>
    /// The maximum amount of time, in seconds, to
    /// allow for the dockerised elasticsearch to
    /// become available before giving up.
    /// When a connection cannot be established within this time,
    /// an UnableToStartTempElasticsearch exception is raised
    /// default: 45
    /// </summary>
    public int MaxTimeAllowedToComeUpInSeconds { get; set; } = 45;

    /// <summary>
    /// Normally, docker output is discarded (there is a lot!)
    /// but if you want to debug the docker startup process,
    /// you may set an handler here to collect logs
    /// - when not set, logs are discarded
    /// </summary>
    public Action<string> DockerLogReceiver { get; set; }
}