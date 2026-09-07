namespace AiEngineering.RAG.Models.RAG;

public sealed class RagSource
{
    public string FileName { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public double Score { get; set; }
    public string Content { get; set; } = string.Empty;
}