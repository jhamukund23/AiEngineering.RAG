using AiEngineering.RAG.Models.RAG;
using Microsoft.Extensions.AI;

namespace AiEngineering.RAG.Services.RAG.VectorStore;

public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly List<VectorDocument> _documents = [];

    public Task AddAsync(
        VectorDocument document,
        CancellationToken cancellationToken = default)
    {
        _documents.Add(document);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        Embedding<float> queryEmbedding,
        int topK = 3,
        CancellationToken cancellationToken = default)
    {
        var results = _documents
            .Select(document => new VectorSearchResult
            {
                Document = document,
                Score = CosineSimilarity(
                    queryEmbedding.Vector.Span,
                    document.Embedding.Vector.Span)
            })
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();

        return Task.FromResult<
            IReadOnlyList<VectorSearchResult>>(results);
    }

    private static double CosineSimilarity(
        ReadOnlySpan<float> vectorA,
        ReadOnlySpan<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length)
        {
            throw new ArgumentException(
                "Vectors must have the same dimensions.");
        }

        double dotProduct = 0;
        double magnitudeA = 0;
        double magnitudeB = 0;

        for (var i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];

            magnitudeA += vectorA[i] * vectorA[i];

            magnitudeB += vectorB[i] * vectorB[i];
        }

        if (magnitudeA == 0 || magnitudeB == 0)
        {
            return 0;
        }

        return dotProduct /
               (Math.Sqrt(magnitudeA) *
                Math.Sqrt(magnitudeB));
    }
}