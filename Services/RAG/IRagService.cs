using AiEngineering.RAG.Models.RAG;

namespace AiEngineering.RAG.Services.RAG;

public interface IRagService
{
    Task<RagResponse> AskAsync(string question, CancellationToken cancellationToken = default);
}