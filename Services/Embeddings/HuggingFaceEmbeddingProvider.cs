using AiEngineering.RAG.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace AiEngineering.RAG.Services.Embeddings;

public sealed class HuggingFaceEmbeddingProvider(
    HttpClient httpClient,
    IOptions<EmbeddingOptions> options)
    : IEmbeddingProvider
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly EmbeddingOptions _options = options.Value;

    public async Task<Embedding<float>> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"{_options.BaseUrl}/{_options.Model}/pipeline/feature-extraction";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            url);

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                _options.ApiKey);

        request.Content = JsonContent.Create(new
        {
            inputs = text
        });

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var vector = await response.Content
            .ReadFromJsonAsync<float[]>(
                cancellationToken);

        if (vector is null || vector.Length == 0)
        {
            throw new InvalidOperationException(
                "Embedding provider returned an empty vector.");
        }

        return new Embedding<float>(vector);
    }
}