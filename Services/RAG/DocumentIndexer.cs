using AiEngineering.RAG.Models.RAG;
using AiEngineering.RAG.Services.RAG.VectorStore;

namespace AiEngineering.RAG.Services.RAG;

public sealed class DocumentIndexer(
    IEmbeddingService embeddingService,
    IVectorStore vectorStore,
    DocumentChunker documentChunker)
    : IDocumentIndexer
{
    private readonly IEmbeddingService _embeddingService =
        embeddingService;

    private readonly IVectorStore _vectorStore =
        vectorStore;

    private readonly DocumentChunker _documentChunker =
        documentChunker;

    public async Task<int> IndexAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (file.Length == 0)
        {
            throw new ArgumentException(
                "Uploaded file is empty.");
        }

        // Read uploaded document
        using var reader =
            new StreamReader(file.OpenReadStream());

        var content = await reader.ReadToEndAsync(
            cancellationToken);

        // Split document into chunks
        var chunks = _documentChunker.Chunk(content);

        // Clean filename once
        var fileName = file.FileName.Trim().Trim('"');

        for (var i = 0; i < chunks.Count; i++)
        {
            // Clean chunk content
            var cleanChunk = chunks[i]
                .Trim()
                .Trim('"');

            // Generate embedding
            var embedding =
                await _embeddingService.GenerateAsync(
                    cleanChunk,
                    cancellationToken);

            // Create vector document
            var vectorDocument = new VectorDocument
            {
                Id = $"{fileName}-{i}",

                FileName = fileName,

                ChunkIndex = i,

                Content = cleanChunk,

                Embedding = embedding
            };

            // Store in Pinecone
            await _vectorStore.AddAsync(
                vectorDocument,
                cancellationToken);
        }

        return chunks.Count;
    }
}