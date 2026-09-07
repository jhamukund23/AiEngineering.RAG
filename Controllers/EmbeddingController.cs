using AiEngineering.RAG.Models.RAG;
using AiEngineering.RAG.Services.RAG;
using Microsoft.AspNetCore.Mvc;

namespace AiEngineering.RAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmbeddingController(IEmbeddingService embeddingService)
    : ControllerBase
{
    private readonly IEmbeddingService _embeddingService = embeddingService;

    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] EmbeddingRequest request, CancellationToken cancellationToken)
    {
        var embedding = await _embeddingService.GenerateAsync(request.Text, cancellationToken);

        return Ok(new
        {
            Dimensions = embedding.Vector.Length,
            Vector = embedding.Vector.ToArray()
        });
    }
}