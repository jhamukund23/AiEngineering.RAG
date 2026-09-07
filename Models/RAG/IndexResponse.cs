namespace AiEngineering.RAG.Models.RAG;

public sealed class IndexResponse
{
    public string Message { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public int Chunks { get; set; }
}
