using AiEngineering.RAG.Models.RAG;
using AiEngineering.RAG.Services.RAG;
using Microsoft.AspNetCore.Mvc;

namespace AiEngineering.RAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RagController(IRagService ragService) : ControllerBase
{
    private readonly IRagService _ragService = ragService;

    [HttpPost("ask")]
    public async Task<ActionResult<RagResponse>> Ask(RagRequest request, CancellationToken cancellationToken)
    {
        var response = await _ragService.AskAsync(request.Question, cancellationToken);

        return Ok(response);
    }
}