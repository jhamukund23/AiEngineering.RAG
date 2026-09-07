using AiEngineering.RAG.Services.RAG.Embeddings;

namespace AiEngineering.RAG.Services.Embeddings;

public sealed class EmbeddingProviderFactory(
    IServiceProvider serviceProvider)
    : IEmbeddingProviderFactory
{
    private readonly IServiceProvider _serviceProvider =
        serviceProvider;

    public IEmbeddingProvider Create(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "huggingface" =>
                _serviceProvider
                    .GetRequiredService<HuggingFaceEmbeddingProvider>(),

            "openai" =>
                _serviceProvider
                    .GetRequiredService<OpenAIEmbeddingProvider>(),        

            _ => throw new InvalidOperationException(
                $"Unsupported embedding provider: {provider}")
        };
    }
}