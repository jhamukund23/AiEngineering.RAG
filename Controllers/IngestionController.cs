using AiEngineering.RAG.Models.RAG;
using AiEngineering.RAG.Services.RAG;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AiEngineering.RAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IngestionController : ControllerBase
{
    private readonly IDocumentIndexer _documentIndexer;
    private readonly ILogger<IngestionController> _logger;

    public IngestionController(
        IDocumentIndexer documentIndexer,
        ILogger<IngestionController> logger)
    {
        _documentIndexer = documentIndexer ?? throw new ArgumentNullException(nameof(documentIndexer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(IndexResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Index([FromForm] FileUploadRequest request, CancellationToken cancellationToken)
    {
        var file = request?.File;

        if (file is null)
        {
            return BadRequest(new { error = "Please upload a document." });
        }

        try
        {
            _logger.LogInformation("Starting indexing for file {FileName} (size: {Size} bytes)", file.FileName, file.Length);

            var chunkCount = await _documentIndexer.IndexAsync(file, cancellationToken);

            var response = new IndexResponse
            {
                Message = "Document indexed successfully.",
                FileName = file.FileName,
                Chunks = chunkCount
            };

            _logger.LogInformation("Completed indexing for file {FileName}. Chunks: {ChunkCount}", file.FileName, chunkCount);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while indexing file {FileName}", file.FileName);
            return BadRequest(new { error = ex.Message });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Indexing cancelled for file {FileName}", file.FileName);
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error while indexing file {FileName}", file.FileName);
            return Problem(title: "Failed to index document.");
        }
    }
}
