namespace AiEngineering.RAG.Configuration;

public sealed class PromptOptions
{
    public string TicketAnalysis { get; set; } = string.Empty;

    public string ZeroShot { get; set; } = string.Empty;

    public string RAG { get; set; } = string.Empty;
}