namespace AiEngineering.RAG.Models.RAG;

public sealed class VectorSearchResult
{
    public VectorDocument Document { get; set; } = null!;

    public double Score { get; set; }
}