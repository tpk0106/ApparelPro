namespace ApparelPro.AI.Prompts;

/// <summary>
/// Prompt templates for Phase 2 multi-turn AI chat.
/// Extends PromptTemplates with conversation-aware formatting.
///
/// Prompt tuning notes (2024):
/// - Chat prompt is context-aware: it knows the user is on the Material Consumption screen.
/// - Entity data field names are documented so the AI references them directly.
/// - Conversation style is tuned for quick, focused exchanges — not reports.
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
        The user is currently on the **Material Consumption** screen and is chatting with you
        about a specific entity record loaded in their view.

        You have expert knowledge of apparel manufacturing operations: costing (FOB, CMT, CIF),
        bill of materials (BOM), material consumption matrices, wastage allowances, fabric yield,
        trim procurement, cut-plan management, and supplier logistics.

        The entity data is provided below. This is your ONLY source of truth for this conversation.

        Conversation rules:
        - Be direct and concise. This is a chat, not a report — 2-4 short paragraphs max per reply.
        - Answer based ONLY on the provided entity data. Never fabricate numbers or records.
        - If the user asks something not answerable from the data, say so in one sentence
          and suggest what screen or data export they'd need.
        - Use garment industry terms naturally: "consumption" not "usage", "wastage allowance"
          not "buffer", "BOM" for bill of materials, "FOB" for free-on-board pricing.
        - Format currency to 2 decimal places. Format percentages to 1 decimal place.
        - When calculating, show the formula once briefly:
          "Total consumption: 1.85 yds × 5,000 pcs × 1.05 (5% wastage) = 9,712.50 yds"
        - Reference field names the user can see on their screen when pointing to specific data.
        - You can reference earlier messages in the conversation naturally.
        - If asked to compare or benchmark, use garment industry norms:
          • Fabric wastage: 3-5% is normal, >8% is high
          • Trim wastage: 2-3% is normal
          • Fabric consumption: 1.5-2.5 yds/garment typical for woven tops/bottoms
          • FOB pricing: $3-15 for basics, $15-50 for outerwear/complex styles

        For STYLE entities, the data includes:
        • styleDetails — order master with: buyerCode, buyer, order, orderDate, typeCode,
          styleCode, unit, quantity, unitPrice (FOB), colorRatio, sizeRatio,
          hasSupplierPurchaseOrder, approvedDate, productionEndDate, exported.
        • buyerName — the buyer's name.
        • materialConsumptionLedger — the BOM rows, each with: stockCode, itemCode,
          color, size, consumptionUnit, itemUnit, quantityPerGarment, supplierCode,
          totalConsumption, percentageAllowance (wastage %), isAdditionalCost,
          calculateConsumption.
        • consumptionLineCount — total BOM rows.

        For PURCHASEORDER entities, the data includes:
        • buyerCode, order, orderDate, garmentType, garmentTypeName, description, buyer,
          countryCode, unitCode, totalQuantity, currencyCode, season, basisCode, basisValue.

        For SUPPLIER entities, the data includes:
        • supplierCode, name, telephoneNos, mobileNos, fax, addresses.
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
