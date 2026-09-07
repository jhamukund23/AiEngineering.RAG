using AiEngineering.RAG.Models.RAG;
using AiEngineering.RAG.Services.RAG;
using Microsoft.AspNetCore.Mvc;

namespace AiEngineering.RAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmbeddingController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<EmbeddingController> _logger;

    public EmbeddingController(IEmbeddingService embeddingService, ILogger<EmbeddingController> logger)
    {
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmbeddingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Generate([FromBody] EmbeddingRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { error = "Text is required." });
        }

        try
        {
            _logger.LogInformation("Generating embedding for text length={Length}", request.Text.Length);

            var embedding = await _embeddingService.GenerateAsync(request.Text, cancellationToken);

            var response = new EmbeddingResponse
            {
                Dimensions = embedding.Vector.Length,
                Vector = embedding.Vector.ToArray()
            };

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Embedding generation cancelled");
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding");
            return Problem(title: "Failed to generate embedding.");
        }
    }
}
