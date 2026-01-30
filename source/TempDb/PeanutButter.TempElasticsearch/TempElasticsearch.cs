using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using PeanutButter.Utils;

namespace PeanutButter.TempElasticsearch;

/// <summary>
/// Provides a temporary Elasticsearch service
/// </summary>
public interface ITempElasticsearch : IDisposable
{
    /// <summary>
    /// Self-assigned Id, used in container name generation
    /// </summary>
    long Id { get; }

    /// <summary>
    /// Read-only copy of the options this instance was launched
    /// with
    /// </summary>
    ReadOnlyTempElasticsearchOptions Options { get; }

    /// <summary>
    /// Public url this service should be available at once running
    /// </summary>
    Uri Url { get; }

    /// <summary>
    /// Recorded time it took to boot the docker container
    /// - does not include the time taken to pull the image
    /// </summary>
    TimeSpan? BootTime { get; }

    /// <summary>
    /// Recorded time it took to pull the docker image
    /// </summary>
    TimeSpan? PullTime { get; }

    /// <summary>
    /// The name assigned to the docker container
    /// </summary>
    string ContainerName { get; }
}

/// <inheritdoc />
public class TempElasticsearch : ITempElasticsearch
{
    /// <inheritdoc />
    public long Id { get; } = DateTime.Now.Ticks;

    /// <inheritdoc />
    public ReadOnlyTempElasticsearchOptions Options { get; }

    /// <inheritdoc />
    public Uri Url { get; private set; }

    /// <inheritdoc />
    public TimeSpan? BootTime { get; private set; }

    /// <inheritdoc />
    public TimeSpan? PullTime { get; private set; }

    /// <inheritdoc />
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
        ValidateConnection(logReceiver);
    }

    private void PullImage(
        ReadOnlyTempElasticsearchOptions options,
        Action<string> dockerLogReceiver
    )
    {
        var image = $"elasticsearch:{options.Version}";
        Console.Error.WriteLine(
            $"""
             Pulling docker image {image} - please be patient, this may take a while"
             """
        );
        var allOutput = new List<string>();
        var started = DateTime.Now;
        using var io = ProcessIO
            .WithStdErrReceiver(StoreLogs)
            .WithStdOutReceiver(StoreLogs)
            .Start(
                "docker",
                "image",
                "pull",
                image
            );

        io.WaitForExit();

        PullTime = DateTime.Now - started;

        if (io.ExitCode != 0)
        {
            throw new UnableToStartTempElasticsearch(
                $"""
                 Unable to pull image '{image}':
                 {allOutput.JoinWith("\n")}
                 """
            );
        }

        void StoreLogs(string str)
        {
            allOutput.Add(str);
            dockerLogReceiver(str);
        }
    }

    private void StartContainer(Action<string> ioReceiver)
    {
        ContainerName = $"temp_es_{Id.ToString().Replace("-", "").ToLower()}";
        Console.Error.WriteLine(
            $"""
             Starting elasticsearch docker container '{ContainerName}'
             """
        );
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

    private void ValidateConnection(Action<string> logReceiver)
    {
        try
        {
            var now = DateTime.Now;
            var eol = now.AddSeconds(Options.MaxTimeAllowedToComeUpInSeconds); // may be too long?
            bool canConnect;
            var httpClient = new HttpClient();
            while (!(canConnect = CanConnect(httpClient, logReceiver)) && DateTime.Now < eol)
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

    private bool CanConnect(HttpClient client, Action<string> logReceiver)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                Url
            );

            var task = client.SendAsync(request);
            task.ConfigureAwait(false);
            var response = task.Result;
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            logReceiver?.Invoke(
                $"""
                 Attempt to connect during boot fails: {ex}
                 """
            );
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