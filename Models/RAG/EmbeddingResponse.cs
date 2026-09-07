namespace AiEngineering.RAG.Models.RAG;

public sealed class EmbeddingResponse
{
    public int Dimensions { get; set; }

    public float[] Vector { get; set; } = Array.Empty<float>();
}
