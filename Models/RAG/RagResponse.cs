namespace AiEngineering.RAG.Models.RAG;

public sealed class RagResponse
{
    public string Answer { get; set; } = string.Empty;

    public List<RagSource> Sources { get; set; } = [];
}