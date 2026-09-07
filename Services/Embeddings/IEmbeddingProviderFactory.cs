using AiEngineering.RAG.Services.Embeddings;

namespace AiEngineering.RAG.Services.RAG.Embeddings;

public interface IEmbeddingProviderFactory
{
    IEmbeddingProvider Create(string provider);
}