using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
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
    public int Port { get; set; } = 9200;

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

/// <summary>
/// Provides a temporary Elasticsearch service
/// </summary>
public class TempElasticsearch : IDisposable
{
    /// <summary>
    /// Self-assigned Id, used in container name generation
    /// </summary>
    public long Id { get; } = DateTime.Now.Ticks;
    /// <summary>
    /// Read-only copy of the options this instance was launched
    /// with
    /// </summary>
    public ReadOnlyTempElasticsearchOptions Options { get; }
    /// <summary>
    /// Public url this service should be available at once running
    /// </summary>
    public Uri Url { get; private set; }
    /// <summary>
    /// Recorded time it took to boot the docker container
    /// - does not include the time taken to pull the image
    /// </summary>
    public TimeSpan? BootTime { get; private set; }
    /// <summary>
    /// The name assigned to the docker container
    /// </summary>
    public string ContainerName { get; private set; }
    
    private IProcessIO _io;

    /// <inheritdoc />
    public TempElasticsearch() : this(9200)
    {
    }

    /// <inheritdoc />
    public TempElasticsearch(
        int port
    ) : this(port, "", "")
    {
    }

    /// <inheritdoc />
    public TempElasticsearch(
        int port,
        string user,
        string password
    ) : this(
        new TempElasticSearchOptions()
        {
            Port = port,
            User = user,
            Password = password
        }
    )
    {
    }

    /// <summary>
    /// Constructs the temporary Elasticsearch service
    /// with the provided options. Will be called from
    /// the "simpler" constructors.
    /// </summary>
    /// <param name="options"></param>
    public TempElasticsearch(
        TempElasticSearchOptions options
    )
    {
        VerifyHaveDockerInPath();
        Options = ReadOnlyTempElasticsearchOptions.From(options);
        var logReceiver = options.DockerLogReceiver ?? NoOp;
        PullImage(Options, logReceiver);
        StartContainer(logReceiver);
        ValidateConnection();
    }

    private void PullImage(
        ReadOnlyTempElasticsearchOptions options,
        Action<string> dockerLogReceiver
    )
    {
        var image = $"elasticsearch:{options.Version}";
        using var io = ProcessIO
            .WithStdErrReceiver(dockerLogReceiver)
            .WithStdOutReceiver(dockerLogReceiver)
            .Start(
                "docker",
                "image",
                "pull",
                image
            );
        io.WaitForExit();
        if (io.ExitCode != 0)
        {
            throw new UnableToStartTempElasticsearch(
                $"""
                 Unable to pull image '{image}':
                 {io.StandardOutputAndErrorInterleavedSnapshot}
                 """
            );
        }
    }

    private void StartContainer(Action<string> ioReceiver)
    {
        ContainerName = $"temp_es_{Id.ToString().Replace("-", "").ToLower()}";
        var args = new List<string>()
        {
            "run",
            "--rm",
            "--name",
            ContainerName,
            "-p",
            $"{Options.Port}:{Options.Port}",
            "-e",
            "discovery.type=single-node",
            "-e",
            "xpack.security.enabled=false",
            "-e",
            $"http.port={Options.Port}",
            "--cpus",
            "2",
            "--memory",
            $"{Options.MaxMemoryMb}m"
        };
        if (!string.IsNullOrWhiteSpace(Options.User))
        {
            args.AddRange(
                [
                    "-e", $"ELASTIC_USERNAME={Options.User}",
                    "-e", $"ELASTIC_PASSWORD={Options.Password}"
                ]
            );
        }

        args.Add($"elasticsearch:{Options.Version}");

        _io = ProcessIO
            .WithStdErrReceiver(ioReceiver)
            .WithStdOutReceiver(ioReceiver)
            .Start(
                "docker",
                args.ToArray()
            );
        Url = new Uri($"http://localhost:{Options.Port}");
    }

    private void VerifyHaveDockerInPath()
    {
        if (string.IsNullOrWhiteSpace(Find.InPath("docker")))
        {
            throw new UnableToStartTempElasticsearch(
                """
                TempElasticsearch relies on docker, which was not found in your PATH
                """
            );
        }
    }

    private void NoOp(string str)
    {
        // intentionally left blank
    }

    private void ValidateConnection()
    {
        try
        {
            var now = DateTime.Now;
            var eol = now.AddSeconds(Options.MaxTimeAllowedToComeUpInSeconds); // may be too long?
            bool canConnect;
            var httpClient = new HttpClient();
            while (!(canConnect = CanConnect(httpClient)) && DateTime.Now < eol)
            {
                Thread.Sleep(500);
                if (_io.HasExited)
                {
                    throw new UnableToStartTempElasticsearch(
                        $"""
                         Error starting up docker:
                         command:
                         {_io.Commandline}
                         output:
                         {_io.StandardOutputAndErrorInterleavedSnapshot.JoinWith("\n")}
                         """
                    );
                }
            }

            if (!canConnect)
            {
                throw new UnableToStartTempElasticsearch(
                    $"""
                     Could not verify connection within {Options.MaxTimeAllowedToComeUpInSeconds}s
                     commandline: {_io.Commandline}
                     """
                );
            }

            BootTime = DateTime.Now - now;
        }
        catch (Exception ex)
        {
            throw new UnableToStartTempElasticsearch(ex);
        }
    }

    private bool CanConnect(HttpClient client)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                Url
            );
            
            var response = Async.RunSync(() => client.SendAsync(request));
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        var io = _io;
        _io = null;
        if (io is not null)
        {
            io.Kill();
            io.Dispose();
        }

        KillContainer();
    }

    private void KillContainer()
    {
        try
        {
            using var io = ProcessIO.Start(
                "docker",
                "kill",
                ContainerName
            );
            io.WaitForExit();
            if (io.ExitCode == 0 || io.ExitCode == 1)
            {
                // 0 is success
                // 1 is what we get if the container isn't running
                // -> which is fine: that's what we want
                return;
            }

            Console.Error.WriteLine(
                $"""
                 WARNING: unable to verify docker container {ContainerName} stopped:
                 docker kill exit code: {io.ExitCode}
                 docker kill output:
                 {io.StandardOutputAndErrorInterleavedSnapshot}
                 """
            );
        }
        catch
        {
            Console.Error.WriteLine("WARNING: unable to stop docker container");
        }
    }
}

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