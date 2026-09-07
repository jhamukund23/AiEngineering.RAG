using AiEngineering.RAG.Services.Embeddings;
using Microsoft.Extensions.AI;

namespace AiEngineering.RAG.Services.RAG;

public sealed class EmbeddingService(
    IEmbeddingProvider embeddingProvider)
    : IEmbeddingService
{
    private readonly IEmbeddingProvider _embeddingProvider =
        embeddingProvider;

    public Task<Embedding<float>> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        return _embeddingProvider.GenerateAsync(
            text,
            cancellationToken);
    }
}