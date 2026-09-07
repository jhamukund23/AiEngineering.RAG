namespace AiEngineering.RAG.Services.RAG;

public sealed class DocumentChunker
{
    public IReadOnlyList<string> Chunk(
        string document,
        int maxChunkSize = 500,
        int overlapSize = 100)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            return [];
        }

        if (overlapSize >= maxChunkSize)
        {
            throw new ArgumentException(
                "Overlap size must be smaller than chunk size.");
        }

        var sentences = document
            .Split(
                ['.', '!', '?'],
                StringSplitOptions.RemoveEmptyEntries)
            .Select(sentence => sentence.Trim())
            .Where(sentence => !string.IsNullOrWhiteSpace(sentence))
            .Select(sentence => sentence + ".")
            .ToList();

        var chunks = new List<string>();

        var currentChunk = string.Empty;

        foreach (var sentence in sentences)
        {
            if (currentChunk.Length == 0)
            {
                currentChunk = sentence;
                continue;
            }

            if (currentChunk.Length + sentence.Length <= maxChunkSize)
            {
                currentChunk += " " + sentence;
                continue;
            }

            chunks.Add(currentChunk);

            var overlap = GetOverlap(
                currentChunk,
                overlapSize);

            currentChunk = overlap + " " + sentence;
        }

        if (!string.IsNullOrWhiteSpace(currentChunk))
        {
            chunks.Add(currentChunk.Trim());
        }

        return chunks;
    }

    private static string GetOverlap(
        string text,
        int overlapSize)
    {
        if (text.Length <= overlapSize)
        {
            return text;
        }

        return text[^overlapSize..];
    }
}