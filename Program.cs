using AiEngineering.RAG.Configuration;
using AiEngineering.RAG.Services.Embeddings;
using AiEngineering.RAG.Services.RAG;
using AiEngineering.RAG.Services.RAG.VectorStore;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Controllers
// --------------------------------------------------

builder.Services.AddControllers();

// --------------------------------------------------
// Swagger / OpenAPI
// --------------------------------------------------

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<VectorStoreOptions>(builder.Configuration.GetSection("VectorStore"));


// --------------------------------------------------
// AI Configuration
// --------------------------------------------------

// Bind AI options from configuration and register for DI
builder.Services.Configure<AIOptions>(builder.Configuration.GetSection("AI"));

// --------------------------------------------------
// Embedding Configuration
// --------------------------------------------------

// Embedding options already configured earlier; ensure binding is present
builder.Services.Configure<EmbeddingOptions>(builder.Configuration.GetSection("Embedding"));

// --------------------------------------------------
// Create OpenAI-compatible Chat Client
// --------------------------------------------------

// Register OpenAI client and chat client using DI so they can be replaced/mocked in tests
builder.Services.AddSingleton(provider =>
{
    var aiOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AIOptions>>().Value;
    return new OpenAIClient(
        new System.ClientModel.ApiKeyCredential(aiOptions.ApiKey),
        new OpenAIClientOptions { Endpoint = new Uri(aiOptions.BaseUrl) });
});

builder.Services.AddSingleton<IChatClient>(provider =>
{
    var aiOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AIOptions>>().Value;
    var client = provider.GetRequiredService<OpenAIClient>();
    return client.GetChatClient(aiOptions.Model).AsIChatClient();
});

// --------------------------------------------------
// Embedding Providers
// --------------------------------------------------

// Hugging Face
builder.Services.AddHttpClient<HuggingFaceEmbeddingProvider>();

// OpenAI
builder.Services.AddHttpClient<OpenAIEmbeddingProvider>();

// --------------------------------------------------
// Embedding Provider
// --------------------------------------------------

// For now: Hugging Face
//
// Later we can select OpenAI / Ollama / Azure OpenAI
// through configuration without changing EmbeddingService.

builder.Services.AddScoped<IEmbeddingProvider, HuggingFaceEmbeddingProvider>();

// --------------------------------------------------
// RAG Services
// --------------------------------------------------

builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
//builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
builder.Services.AddSingleton<IVectorStore, PineconeVectorStore>();
builder.Services.AddSingleton<DocumentChunker>();

builder.Services.AddScoped<IDocumentIndexer, DocumentIndexer>();

// --------------------------------------------------
// Build Application
// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------
// HTTP Request Pipeline
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
