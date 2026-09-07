using Microsoft.AspNetCore.Http;

namespace AiEngineering.RAG.Models.RAG;

public sealed class FileUploadRequest
{
    public IFormFile? File { get; set; }
}
