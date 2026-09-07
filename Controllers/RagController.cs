using AiEngineering.RAG.Models.RAG;
using AiEngineering.RAG.Services.RAG;
using Microsoft.AspNetCore.Mvc;

namespace AiEngineering.RAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RagController : ControllerBase
{
    private readonly IRagService _ragService;
    private readonly ILogger<RagController> _logger;

    public RagController(IRagService ragService, ILogger<RagController> logger)
    {
        _ragService = ragService ?? throw new ArgumentNullException(nameof(ragService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("ask")]
    [ProducesResponseType(typeof(RagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RagResponse>> Ask([FromBody] RagRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new { error = "Question is required." });
        }

        try
        {
            _logger.LogInformation("Processing RAG question (length={Length})", request.Question.Length);
            var response = await _ragService.AskAsync(request.Question, cancellationToken);
            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RAG request cancelled");
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RAG request");
            return Problem(title: "Failed to process request.");
        }
    }
}
