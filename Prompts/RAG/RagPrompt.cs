namespace AiEngineering.RAG.Prompts.RAG;

public static class RagPrompt
{
    public static string Build(
        string question,
        string context)
    {
        return $$"""
            You are a customer support assistant.

            Answer the user's question using ONLY the information
            provided in the context.

            If the answer cannot be found in the context,
            respond with:
            "I don't know based on the available information."

            Context:
            {{context}}

            User question:
            {{question}}
            """;
    }
}