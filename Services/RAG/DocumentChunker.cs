using Microsoft.Extensions.Logging;

namespace AiEngineering.RAG.Services.RAG;

/// <summary>
/// Splits a document into chunks suitable for embedding and vector storage.
/// The implementation prefers sentence boundaries and falls back to word-based
/// splits for very long sentences. Overlap is applied on word boundaries to
/// avoid cutting words in half.
/// </summary>
public sealed class DocumentChunker
{
    private readonly ILogger<DocumentChunker> _logger;

    public DocumentChunker(ILogger<DocumentChunker> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IReadOnlyList<string> Chunk(
        string document,
        int maxChunkSize = 500,
        int overlapSize = 100)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return Array.Empty<string>();
        }

        if (maxChunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxChunkSize));
        if (overlapSize < 0) throw new ArgumentOutOfRangeException(nameof(overlapSize));
        if (overlapSize >= maxChunkSize)
            throw new ArgumentException("Overlap size must be smaller than chunk size.", nameof(overlapSize));

        // Naive sentence split using punctuation. This is intentionally simple
        // to avoid external tokenizer dependencies. It handles most natural
        // sentences and falls back to word-splitting for very long sentences.
        var sentenceSeparators = new[] { '.', '!', '?' };

        var sentences = document
            .Split(sentenceSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s + ".")
            .ToList();

        var chunks = new List<string>();

        var currentWords = new List<string>();
        var currentLength = 0;

        static IEnumerable<string> SplitToWords(string text)
            => text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var sentence in sentences)
        {
            var words = SplitToWords(sentence).ToList();

            // If the sentence itself is larger than maxChunkSize, split it by words
            if (sentence.Length > maxChunkSize)
            {
                foreach (var word in words)
                {
                    if (currentLength == 0)
                    {
                        currentWords.Add(word);
                        currentLength = word.Length;
                        continue;
                    }

                    if (currentLength + 1 + word.Length <= maxChunkSize)
                    {
                        currentWords.Add(word);
                        currentLength += 1 + word.Length;
                        continue;
                    }

                    // Commit current chunk
                    chunks.Add(string.Join(' ', currentWords).Trim());

                    // Start new chunk with overlap
                    var overlapWords = GetOverlapWords(currentWords, overlapSize);
                    currentWords = new List<string>(overlapWords);
                    currentWords.Add(word);
                    currentLength = string.Join(' ', currentWords).Length;
                }

                continue;
            }

            // Try to append the whole sentence to current chunk
            if (currentLength == 0)
            {
                currentWords.AddRange(words);
                currentLength = string.Join(' ', currentWords).Length;
                continue;
            }

            var projectedLength = currentLength + 1 + sentence.Length;
            if (projectedLength <= maxChunkSize)
            {
                currentWords.AddRange(words);
                currentLength = projectedLength;
                continue;
            }

            // Commit current chunk and start new with overlap
            chunks.Add(string.Join(' ', currentWords).Trim());

            var overlapWords2 = GetOverlapWords(currentWords, overlapSize);
            currentWords = new List<string>(overlapWords2);
            currentWords.AddRange(words);
            currentLength = string.Join(' ', currentWords).Length;
        }

        if (currentWords.Count > 0)
        {
            chunks.Add(string.Join(' ', currentWords).Trim());
        }

        _logger.LogDebug("Document chunked into {ChunkCount} chunks (maxChunkSize={MaxChunkSize}, overlapSize={OverlapSize})", chunks.Count, maxChunkSize, overlapSize);

        return chunks;
    }

    private static IEnumerable<string> GetOverlapWords(IReadOnlyList<string> words, int overlapSize)
    {
        if (words.Count == 0 || overlapSize <= 0) yield break;

        // Build overlap string until we reach overlapSize in characters
        var overlap = new List<string>();
        var length = 0;

        for (var i = words.Count - 1; i >= 0; i--)
        {
            var word = words[i];
            var addLength = (length == 0 ? word.Length : 1 + word.Length);
            if (length + addLength > overlapSize)
            {
                break;
            }

            overlap.Insert(0, word);
            length += addLength;
        }

        foreach (var w in overlap) yield return w;
    }
}
