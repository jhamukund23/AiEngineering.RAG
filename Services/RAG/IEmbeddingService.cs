using Microsoft.Extensions.AI;

namespace AiEngineering.RAG.Services.RAG;

public interface IEmbeddingService
{
    Task<Embedding<float>> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default);
}