namespace AiEngineering.RAG.Configuration;

public sealed class AIOptions
{
    public string Provider { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public PromptOptions Prompts { get; set; } = new();

    public string ApiKey { get; set; } = string.Empty;
}
