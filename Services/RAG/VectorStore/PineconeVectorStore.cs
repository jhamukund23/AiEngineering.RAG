using AiEngineering.RAG.Configuration;
using AiEngineering.RAG.Models.RAG;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pinecone;

namespace AiEngineering.RAG.Services.RAG.VectorStore;

/// <summary>
/// Pinecone-backed implementation of IVectorStore.
/// Uses a PineconeClient provided by DI, performs basic validation,
/// and logs key operations for observability. Keep this class thin
/// and focused on mapping domain models to the Pinecone SDK types.
/// </summary>
public sealed class PineconeVectorStore : IVectorStore
{
    private readonly VectorStoreOptions _options;
    private readonly PineconeClient _pineconeClient;
    private readonly ILogger<PineconeVectorStore> _logger;

    public PineconeVectorStore(
        IOptions<VectorStoreOptions> options,
        PineconeClient pineconeClient,
        ILogger<PineconeVectorStore> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _pineconeClient = pineconeClient ?? throw new ArgumentNullException(nameof(pineconeClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(_options.IndexName))
        {
            throw new ArgumentException("VectorStore index name is not configured.", nameof(options));
        }
    }

    public async Task AddAsync(VectorDocument document, CancellationToken cancellationToken = default)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));

        try
        {
            var index = _pineconeClient.Index(_options.IndexName);

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

            _logger.LogDebug("Upserting vector id={VectorId} into index={Index}", document.Id, _options.IndexName);

            await index.UpsertAsync(new UpsertRequest
            {
                Namespace = _options.Namespace,
                Vectors = [vector]
            });

            _logger.LogInformation("Successfully upserted vector id={VectorId} into index={Index}", document.Id, _options.IndexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding vector id={VectorId} to index={Index}", document?.Id, _options.IndexName);
            throw;
        }
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(Microsoft.Extensions.AI.Embedding<float> queryEmbedding, int topK = 3, CancellationToken cancellationToken = default)
    {
        if (queryEmbedding.Vector.Length == 0)
            throw new ArgumentException("Query embedding is empty.", nameof(queryEmbedding));

        try
        {
            var index = _pineconeClient.Index(_options.IndexName);

            var response = await index.QueryAsync(new QueryRequest
            {
                Namespace = _options.Namespace,
                Vector = queryEmbedding.Vector.ToArray(),
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

                results.Add(new VectorSearchResult
                {
                    Document = document,
                    Score = match.Score ?? 0d
                });
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while querying index={Index}", _options.IndexName);
            throw;
        }
    }

    private static string GetMetadataString(Metadata? metadata, string key)
    {
        if (metadata is null || !metadata.TryGetValue(key, out var value))
        {
            return string.Empty;
        }

        var result = value?.ToString() ?? string.Empty;

        if (result.Length >= 2 && result.StartsWith('"') && result.EndsWith('"'))
        {
            result = result[1..^1];
        }

        return result;
    }

    private static int GetMetadataInt(Metadata? metadata, string key)
    {
        if (metadata is null || !metadata.TryGetValue(key, out var value))
        {
            return 0;
        }

        var result = value?.ToString() ?? string.Empty;

        return int.TryParse(result, out var number) ? number : 0;
    }
}
