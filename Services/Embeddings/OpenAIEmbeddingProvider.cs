using AiEngineering.RAG.Services.Embeddings;
using Microsoft.Extensions.AI;

public sealed class OpenAIEmbeddingProvider : IEmbeddingProvider
{
    // implementation
    public Task<Embedding<float>> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}