using AiEngineering.RAG.Models.RAG;
using AiEngineering.RAG.Services.RAG.VectorStore;
using Microsoft.Extensions.Logging;

namespace AiEngineering.RAG.Services.RAG;

public sealed class DocumentIndexer(
    IEmbeddingService embeddingService,
    IVectorStore vectorStore,
    DocumentChunker documentChunker,
    ILogger<DocumentIndexer> logger)
    : IDocumentIndexer
{
    private readonly IEmbeddingService _embeddingService = embeddingService;

    private readonly IVectorStore _vectorStore = vectorStore;

    private readonly DocumentChunker _documentChunker = documentChunker;

    private readonly ILogger<DocumentIndexer> _logger = logger;

    public async Task<int> IndexAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            _logger.LogWarning("IndexAsync called with null file reference.");
            throw new ArgumentNullException(nameof(file));
        }

        if (file.Length == 0)
        {
            _logger.LogWarning("Uploaded file {FileName} is empty.", file.FileName);
            throw new ArgumentException("Uploaded file is empty.");
        }

        _logger.LogInformation("Reading uploaded document {FileName} (size: {Size} bytes)", file.FileName, file.Length);

        // Read uploaded document
        using var reader = new StreamReader(file.OpenReadStream());

        var content = await reader.ReadToEndAsync(cancellationToken);

        // Split document into chunks
        var chunks = _documentChunker.Chunk(content);

        // Clean filename once
        var fileName = file.FileName.Trim().Trim('"');

        for (var i = 0; i < chunks.Count; i++)
        {
            // Clean chunk content
            var cleanChunk = chunks[i].Trim().Trim('"');

            // Generate embedding
            var embedding = await _embeddingService.GenerateAsync(cleanChunk, cancellationToken);

            // Create vector document
            var vectorDocument = new VectorDocument
            {
                Id = $"{fileName}-{i}",
                FileName = fileName,
                ChunkIndex = i,
                Content = cleanChunk,
                Embedding = embedding
            };

            // Store in vector store
            await _vectorStore.AddAsync(vectorDocument, cancellationToken);

            _logger.LogDebug("Stored chunk {ChunkIndex} for file {FileName}", i, fileName);
        }

        _logger.LogInformation("Indexed file {FileName} into {ChunkCount} chunks.", fileName, chunks.Count);

        return chunks.Count;
    }
}
