using System;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using PeanutButter.Utils;
// ReSharper disable UnusedMember.Global

namespace PeanutButter.TempElasticsearch.Tests;

public class TestTempElasticsearch
{
    private TempElasticsearchFactory _factory;
    
    private static bool DebugEnabled =>
        _debugEnabled ??= Env.Flag("ELASTICSEARCH_DEBUG", false);
    private static bool? _debugEnabled;

    private static Action<string> CreateLogger()
    {
        return DebugEnabled
            ? s => Console.Error.WriteLine(s)
            : NoOp;
    }

    private static void NoOp(string str)
    {
        // intentionally left blank
    }

    [Test]
    [Order(1)]
    public async Task _1_ShouldSetUpServer()
    {
        // Arrange
        _factory.Warmup();
        using var lease = _factory.Borrow();
        var tempES = lease.Instance;
        // Act
        var client = new ElasticsearchClient(tempES.Url);
        await Console.Error.WriteLineAsync("-----------------------\n\n\n");
        await Console.Error.WriteLineAsync(
            new
            {
                tempES.PullTime,
                tempES.BootTime,
                tempES.Options
            }.Stringify()
        );
        await Console.Error.WriteLineAsync("\n\n\n-----------------------");
        await RunTestWith(client);
        // Assert
    }

    [Test]
    [Order(2)]
    public async Task _2_ShouldBeFasterSecondTimeAroundFromFactory()
    {
        // Arrange
        using var lease = _factory.Borrow();
        var tempES = lease.Instance;
        // Act
        var client = new ElasticsearchClient(tempES.Url);
        await RunTestWith(client);
        // Assert
    }

    [Test]
    [Explicit("Run against an existing ES instance")]
    public async Task ShouldConnect()
    {
        // Arrange
        // using var lease = _factory.Borrow();
        // var tempES = lease.Instance;
        // Act
        var client = new ElasticsearchClient(
            new Uri("http://localhost:9200")
        );
        await RunTestWith(client);
        // Assert
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var options = new TempElasticSearchOptions()
        {
            DockerLogReceiver = CreateLogger()
        };
        _factory = new TempElasticsearchFactory(10, options);
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        _factory?.Dispose();
        _factory = null;
    }

    private async Task RunTestWith(
        ElasticsearchClient client
    )
    {
        var index = "the-test-index";
        var payload = GetRandom<IndexItem>();
        var existingIndeces = await client.Indices.GetAsync(
            new GetIndexRequest(
                Indices.All
            )
        );
        if (!existingIndeces.Indices.ContainsKey(index))
        {
            await client.Indices.CreateAsync(
                new CreateIndexRequest(index)
            );
        }

        // Act
        var indexResponse = await client.IndexAsync(
            payload,
            o => o.Index(index)
        );
        Expect(indexResponse.IsValidResponse)
            .To.Be.True();
        Log($"Indexed:\n{payload.Stringify()}");
        var foundDoc = false;
        SearchResponse<IndexItem> searchResponse = null;
        var attempts = 0;
        while (!foundDoc && ++attempts < 10)
        {
            // elasticsearch returns immediately after the indexing
            // request, so the document isn't available for up to 1s
            // from what I've read; so this code is both testing that
            // and testing that I can get back out what I put in
            searchResponse = client.Search<IndexItem>(s =>
                s.Indices(index)
                    .Query(q => q.Match(t =>
                            t.Field(o => o.Id)
                                .Query(payload.Id)
                        )
                    )
            );
            if (searchResponse.IsValidResponse)
            {
                foundDoc = searchResponse.Documents.Any(o => o.DeepEquals(payload));
                if (!foundDoc)
                {
                    Log("Indexed document not found yet");
                    await Task.Delay(TimeSpan.FromSeconds(0.1));
                }
            }
            else
            {
                Log("Error performing search");
                await Task.Delay(TimeSpan.FromSeconds(0.5));
            }
        }

        Expect(searchResponse)
            .Not.To.Be.Null();
        Expect(searchResponse!.IsValidResponse)
            .To.Be.True(() => searchResponse.Stringify());
        Log(searchResponse.Documents.Stringify());
        // Assert

        void Log(string str)
        {
            Console.Error.WriteLine($"[{DateTime.Now:u}] {str}");
        }
    }

    public class IndexItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Flag { get; set; }
        public DateTime Date { get; set; }
    }
}