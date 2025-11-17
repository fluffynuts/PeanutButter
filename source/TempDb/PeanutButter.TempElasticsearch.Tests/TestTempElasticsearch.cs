using System;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using PeanutButter.Utils;

namespace PeanutButter.TempElasticsearch.Tests;

public class TestTempElasticsearch
{
    private TempElasticsearchFactory _factory;

    [Test]
    public async Task ShouldSetUpServer()
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
    public async Task ShouldBeFasterSecondTimeAroundFromFactory()
    {
        // Arrange
        using var lease = _factory.Borrow();
        var tempES = lease.Instance;
        // Act
        var client = new ElasticsearchClient(tempES.Url);
        await RunTestWith(client);
        // Assert
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _factory = new TempElasticsearchFactory();
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
        var index = "callcenter-orders";
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
            o => o.Index("callcenter-orders")
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
            Console.WriteLine($"[{DateTime.Now:u}] {str}");
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