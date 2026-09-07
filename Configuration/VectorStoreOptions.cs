namespace AiEngineering.RAG.Configuration;

public sealed class VectorStoreOptions
{
    public string Provider { get; set; } = string.Empty;

    public string IndexName { get; set; } = string.Empty;

    public string Namespace { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}