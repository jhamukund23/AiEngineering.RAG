using Microsoft.AspNetCore.Http;

namespace AiEngineering.RAG.Services.RAG;

public interface IDocumentIndexer
{
    Task<int> IndexAsync(
        IFormFile file,
        CancellationToken cancellationToken = default);
}