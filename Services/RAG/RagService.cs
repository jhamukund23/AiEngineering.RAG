using AiEngineering.RAG.Models.RAG;
using AiEngineering.RAG.Prompts.RAG;
using AiEngineering.RAG.Services.RAG.VectorStore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;

namespace AiEngineering.RAG.Services.RAG;

public sealed class RagService(
    IChatClient chatClient,
    IEmbeddingService embeddingService,
    IVectorStore vectorStore,
    ILogger<RagService> logger) : IRagService
{
    private const int TopK = 5;
    private const double SimilarityThreshold = 0.50;
    private const int MaxContextCharacters = 6000;

    private readonly IChatClient _chatClient = chatClient;

    private readonly IEmbeddingService _embeddingService =
        embeddingService;

    private readonly IVectorStore _vectorStore =
        vectorStore;

    private readonly ILogger<RagService> _logger = logger;

    public async Task<RagResponse> AskAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Question must be provided.", nameof(question));
        }

        _logger.LogInformation("Processing RAG question (length={Length})", question.Length);
        // 1. Convert question into embedding
        var questionEmbedding =
            await _embeddingService.GenerateAsync(
                question,
                cancellationToken);

        // 2. Search vector store
        var searchResults =
            await _vectorStore.SearchAsync(
                questionEmbedding,
                TopK,
                cancellationToken);

        // 3. Apply similarity threshold
        var relevantResults = searchResults
            .Where(x => x.Score >= SimilarityThreshold)
            .ToList();

        // 4. No relevant information
        if (relevantResults.Count == 0)
        {
            _logger.LogInformation("No relevant RAG sources found for question");
            return new RagResponse
            {
                Answer = "I don't know based on the available information.",
                Sources = new List<RagSource>()
            };
        }

        // 5. Build context
        var context = BuildContext(
            relevantResults);

        // 6. Build RAG prompt
        var prompt = RagPrompt.Build(
            question,
            context);

        // 7. Generate answer
        var response =
            await _chatClient.GetResponseAsync(
                prompt,
                cancellationToken: cancellationToken);

        // 8. Build source information
        var sources = relevantResults
          .Select(x => new RagSource
          {
              FileName = x.Document.FileName,
              ChunkIndex = x.Document.ChunkIndex,
              Score = Math.Round(x.Score, 4),
              Content = x.Document.Content
          })
          .ToList();

        return new RagResponse
        {
            Answer = response.Text ?? string.Empty,
            Sources = sources
        };
    }

    private static string BuildContext(
        IReadOnlyList<VectorSearchResult> results)
    {
        var context = new StringBuilder();

        foreach (var result in results)
        {
            var content = result.Document.Content;

            if (context.Length + content.Length >
                MaxContextCharacters)
            {
                break;
            }

            context.AppendLine(content);
            context.AppendLine();
        }

        return context.ToString().Trim();
    }
}
