using AiEngineering.RAG.Configuration;
using AiEngineering.RAG.Models.RAG;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Pinecone;

namespace AiEngineering.RAG.Services.RAG.VectorStore;

public sealed class PineconeVectorStore(
    IOptions<VectorStoreOptions> options)
    : IVectorStore
{
    private readonly VectorStoreOptions _options =
        options.Value;

    private readonly PineconeClient _pineconeClient =
        new(options.Value.ApiKey);

    public async Task AddAsync(
        VectorDocument document,
        CancellationToken cancellationToken = default)
    {
        var index =
            _pineconeClient.Index(_options.IndexName);

        if (document is null) throw new ArgumentNullException(nameof(document));

        var values = document.Embedding.Vector.ToArray();
        if (values.Length == 0)
        {
            throw new ArgumentException("Document embedding is empty.", nameof(document));
        }

        var vector = new Vector
        {
            Id = document.Id,
            Values = values,
            Metadata = new Metadata
            {
                ["fileName"] = document.FileName,
                ["chunkIndex"] = document.ChunkIndex,
                ["content"] = document.Content
            }
        };

        await index.UpsertAsync(
            new UpsertRequest
            {
                Namespace = _options.Namespace,
                Vectors = [vector]
            });
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        Embedding<float> queryEmbedding,
        int topK = 3,
        CancellationToken cancellationToken = default)
    {
        var index =
            _pineconeClient.Index(_options.IndexName);

        var response = await index.QueryAsync(
            new QueryRequest
            {
                Namespace = _options.Namespace,

                Vector = queryEmbedding
                    .Vector
                    .ToArray(),

                TopK = (uint)topK,

                IncludeMetadata = true
            });

        var results = new List<VectorSearchResult>();

        if (response.Matches is null)
        {
            return results;
        }

        foreach (var match in response.Matches)
        {
            var document = new VectorDocument
            {
                Id = match.Id,
                FileName = GetMetadataString(match.Metadata, "fileName"),
                ChunkIndex = GetMetadataInt(match.Metadata, "chunkIndex"),
                Content = GetMetadataString(match.Metadata, "content")
            };

            results.Add(
                new VectorSearchResult
                {
                    Document = document,
                    Score = match.Score ?? 0d
                });
        }

        return results;
    }

    private static string GetMetadataString(
        Metadata? metadata,
        string key)
    {
        if (metadata is null ||
            !metadata.TryGetValue(key, out var value))
        {
            return string.Empty;
        }

        var result = value?.ToString() ?? string.Empty;

        // Remove surrounding quotes returned
        // by Pinecone metadata representation.
        if (result.Length >= 2 &&
            result.StartsWith('"') &&
            result.EndsWith('"'))
        {
            result = result[1..^1];
        }

        return result;
    }

    private static int GetMetadataInt(
        Metadata? metadata,
        string key)
    {
        if (metadata is null ||
            !metadata.TryGetValue(key, out var value))
        {
            return 0;
        }

        var result = value?.ToString() ?? string.Empty;

        return int.TryParse(
            result,
            out var number)
            ? number
            : 0;
    }
}
