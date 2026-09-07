using AiEngineering.RAG.Services.RAG;
using Microsoft.AspNetCore.Mvc;

namespace AiEngineering.RAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IngestionController(IDocumentIndexer documentIndexer) : ControllerBase
{
    private readonly IDocumentIndexer _documentIndexer = documentIndexer;

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Index(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest("Please upload a document.");
        }

        var chunkCount =
            await _documentIndexer.IndexAsync(file, cancellationToken);

        return Ok(new
        {
            message = "Document indexed successfully.",
            fileName = file.FileName,
            chunks = chunkCount
        });
    }
}