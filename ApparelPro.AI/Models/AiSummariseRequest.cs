namespace ApparelPro.AI.Models;

/// <summary>
/// API-level request model for the summarise endpoint.
/// </summary>
public sealed class AiSummariseAPIModel
{
    /// <summary>
    /// The entity type to summarise (e.g. "Style", "PurchaseOrder", "Supplier").
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// The primary key or composite key identifying the entity.
    /// For Style: pass as "BuyerCode/Order/TypeCode/StyleCode".
    /// For PurchaseOrder: pass the PO number.
    /// </summary>
    public required string EntityKey { get; init; }

    /// <summary>
    /// Optional additional context or specific question about the entity.
    /// </summary>
    public string? UserQuery { get; init; }
}
