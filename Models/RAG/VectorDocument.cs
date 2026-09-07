using Microsoft.Extensions.AI;

namespace AiEngineering.RAG.Models.RAG;

public sealed class VectorDocument
{
    public string Id { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    public Embedding<float> Embedding { get; set; } = null!;
}