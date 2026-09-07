using AiEngineering.RAG.Models.RAG;
using Microsoft.Extensions.AI;

namespace AiEngineering.RAG.Services.RAG.VectorStore;

public interface IVectorStore
{
    Task AddAsync(VectorDocument document, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(Embedding<float> queryEmbedding, int topK = 3, CancellationToken cancellationToken = default);
}