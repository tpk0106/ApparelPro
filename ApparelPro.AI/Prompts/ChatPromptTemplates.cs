namespace ApparelPro.AI.Prompts;

/// <summary>
/// Prompt templates for Phase 2 multi-turn AI chat.
/// Extends PromptTemplates with conversation-aware formatting.
/// </summary>
public static class ChatPromptTemplates
{
    // ─────────────────────────────────────────────
    //  CHAT MODE — multi-turn conversation (Phase 2)
    // ─────────────────────────────────────────────

    /// <summary>
    /// System prompt for multi-turn chat sessions.
    /// Sets the AI as an interactive ERP assistant with entity context.
    /// </summary>
    public const string ChatSystem = """
        You are an AI assistant embedded in ApparelPro, a garment and apparel manufacturing ERP system.
        You are having an interactive conversation with a user about a specific entity in their system.

        The entity data is provided below as context. Use it to answer questions accurately.

        Rules:
        - Be conversational but professional. Use the user's first name if known.
        - Answer based ONLY on the provided entity data. Never fabricate data.
        - If the user asks something not answerable from the data, say so clearly
          and suggest what additional data would be needed.
        - Use garment industry terminology where appropriate.
        - Format currency values to 2 decimal places.
        - Format percentages to 1 decimal place.
        - When performing calculations, show your working briefly.
        - Keep responses focused and concise — this is a chat, not a report.
        - If asked to compare or benchmark, use garment industry standards
          (e.g. typical fabric wastage 3-5%, standard lead times).
        - You can reference earlier parts of the conversation naturally.
        """;

    /// <summary>
    /// Builds the full system prompt with entity context injected.
    /// </summary>
    /// <param name="entityType">The entity type (e.g. "STYLE").</param>
    /// <param name="entityDataJson">Serialised JSON of the entity data.</param>
    /// <returns>Complete system prompt with entity context.</returns>
    public static string BuildChatSystemPrompt(string entityType, string entityDataJson)
    {
        return $"""
            {ChatSystem}

            ═══════════════════════════════════════════
            ENTITY CONTEXT: {entityType}
            ═══════════════════════════════════════════
            {entityDataJson}
            ═══════════════════════════════════════════
            """;
    }

    /// <summary>
    /// Formats a conversation history into a single user message for providers
    /// that only support system + single-user message format.
    ///
    /// The conversation is formatted as XML-style tags for clear role separation.
    /// </summary>
    /// <param name="conversationHistory">
    /// Ordered list of (role, content) tuples from oldest to newest.
    /// </param>
    /// <param name="currentMessage">The user's new message.</param>
    /// <returns>Formatted message combining history and current question.</returns>
    public static string BuildConversationMessage(
        IReadOnlyList<(string Role, string Content)> conversationHistory,
        string currentMessage)
    {
        if (conversationHistory.Count == 0)
        {
            return currentMessage;
        }

        var historyBlock = string.Join("\n\n", conversationHistory.Select(msg =>
            $"<message role=\"{msg.Role}\">\n{msg.Content}\n</message>"));

        return $"""
            <conversation_history>
            {historyBlock}
            </conversation_history>

            <current_message>
            {currentMessage}
            </current_message>
            """;
    }

    /// <summary>
    /// Auto-generates a session title from the first user message.
    /// Truncates to fit the 200-char limit with an entity type prefix.
    /// </summary>
    public static string GenerateSessionTitle(string entityType, string entityKey, string firstMessage)
    {
        var prefix = $"{entityType}: {entityKey} — ";
        var maxMessageLength = 200 - prefix.Length;

        if (maxMessageLength <= 0)
            return $"{entityType}: {entityKey}"[..Math.Min(200, $"{entityType}: {entityKey}".Length)];

        var truncatedMessage = firstMessage.Length <= maxMessageLength
            ? firstMessage
            : firstMessage[..(maxMessageLength - 3)] + "...";

        return prefix + truncatedMessage;
    }
}
