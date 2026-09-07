namespace AiEngineering.RAG.Configuration;

public sealed class EmbeddingOptions
{
    public string Provider { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}