using Microsoft.Extensions.AI;

namespace AiEngineering.RAG.Services.Embeddings;

public interface IEmbeddingProvider
{
    Task<Embedding<float>> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default);
}