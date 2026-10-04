using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Models;
using ApparelPro.WebApi.Misc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 CROSS-ENTITY ANALYTICS CONTROLLER
// Phase 2, Feature 1: REST endpoint for cross-entity analytics.
//
// 🎓 WHY A SEPARATE CONTROLLER (not adding to AiController)?
// Single Responsibility Principle: AiController handles single-entity
// summarise/analyse. This controller handles cross-entity analytics —
// different request shape, different service, different response structure.
//
// Endpoint: POST /api/ai/analytics
// Request:  { "category": "MATERIAL_CONSUMPTION", "userQuery": "optional question" }
// Response: { "analysis": "...", "category": "...", "entityCount": 47, ... }
//
// 🎓 AUTHORIZATION:
// Uses the same "style-details" policy as AiController — analytics data
// is derived from styles, orders, and consumption, so anyone who can view
// style details should be able to view analytics. If stricter control is
// needed later, a dedicated "analytics" policy can be added.
// ─────────────────────────────────────────────────────────────────────────────

[Route("api/ai/analytics")]
[ApiController]
[Authorize(Policy = "style-details")]
public class CrossEntityAnalyticsController : ControllerBase
{
    private readonly ICrossEntityAnalyticsService _analyticsService;

    public CrossEntityAnalyticsController(ICrossEntityAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// 🎓 Run a cross-entity analytics analysis for the specified category.
    ///
    /// Supported categories (case-insensitive):
    ///   MATERIAL_CONSUMPTION — consumption patterns, wastage, cost drivers
    ///   SUPPLIER_PERFORMANCE — supplier coverage, concentration, contact gaps
    ///   BUYER_PORTFOLIO      — revenue distribution, order activity, pricing
    ///   ORDER_PIPELINE       — pipeline value, age distribution, garment mix
    ///   COST_ANALYSIS        — cost structures, wastage impact, margin indicators
    ///
    /// Example request body:
    /// {
    ///   "category": "MATERIAL_CONSUMPTION",
    ///   "userQuery": "Which styles have wastage above 8%?"
    /// }
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CrossEntityAnalyticsAPIModel_Response), HttpStatusCodes.OK)]
    [ProducesResponseType(HttpStatusCodes.BadRequest)]
    [ProducesResponseType(HttpStatusCodes.InternalServerError)]
    public async Task<IActionResult> RunAnalyticsAsync(
        [FromBody] CrossEntityAnalyticsAPIModel request,
        CancellationToken cancellationToken)
    {
        // 🎓 VALIDATION: Category is the only required field
        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return BadRequest("Category is required. " +
                "Supported: MATERIAL_CONSUMPTION, SUPPLIER_PERFORMANCE, " +
                "BUYER_PORTFOLIO, ORDER_PIPELINE, COST_ANALYSIS.");
        }

        try
        {
            // 🎓 The service handles everything:
            //   1. Validates the category
            //   2. Aggregates data from EF Core
            //   3. Builds the prompt from AnalyticsPromptTemplates
            //   4. Calls IAiService.CompleteAsync()
            //   5. Returns (AiCompletionResponse, entityCount)
            var (response, entityCount) = await _analyticsService.GetAnalyticsAsync(
                request.Category.Trim(),
                request.UserQuery,
                cancellationToken);

            // 🎓 Map the service response to the API response model
            var result = new CrossEntityAnalyticsAPIModel_Response
            {
                Analysis = response.Content,
                Category = request.Category.Trim().ToUpperInvariant(),
                EntityCount = entityCount,
                Provider = response.Provider,
                Model = response.Model,
                InputTokens = response.InputTokens,
                OutputTokens = response.OutputTokens,
                TotalTokens = response.TotalTokens,
                EstimatedCost = response.EstimatedCost,
                GeneratedAt = DateTimeOffset.UtcNow
            };

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            // 🎓 Thrown by GetAnalyticsTemplate for unsupported categories
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            // 🎓 Thrown when the AI provider HTTP call fails
            return StatusCode(502, $"AI provider request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// 🎓 Returns the list of supported analytics categories.
    /// Useful for the frontend to populate a dropdown/selector.
    ///
    /// GET /api/ai/analytics/categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<AnalyticsCategoryInfo>), HttpStatusCodes.OK)]
    public IActionResult GetCategories()
    {
        // 🎓 Static list — matches AnalyticsPromptTemplates.GetAnalyticsTemplate switch
        var categories = new[]
        {
            new AnalyticsCategoryInfo
            {
                Key = "MATERIAL_CONSUMPTION",
                Name = "Material Consumption",
                Description = "Analyse consumption patterns, wastage, and cost drivers across all styles"
            },
            new AnalyticsCategoryInfo
            {
                Key = "SUPPLIER_PERFORMANCE",
                Name = "Supplier Performance",
                Description = "Analyse supplier coverage, order distribution, and contact completeness"
            },
            new AnalyticsCategoryInfo
            {
                Key = "BUYER_PORTFOLIO",
                Name = "Buyer Portfolio",
                Description = "Analyse buyer order volumes, revenue concentration, and pricing"
            },
            new AnalyticsCategoryInfo
            {
                Key = "ORDER_PIPELINE",
                Name = "Order Pipeline",
                Description = "Analyse purchase order status, value distribution, and timelines"
            },
            new AnalyticsCategoryInfo
            {
                Key = "COST_ANALYSIS",
                Name = "Cost Analysis",
                Description = "Analyse cost structures, pricing patterns, and margin indicators"
            }
        };

        return Ok(categories);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 HELPER MODEL — category info for the GET /categories endpoint
// Kept in the same file because it's tiny and only used by this controller.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Describes an available analytics category for the frontend.
/// </summary>
public sealed class AnalyticsCategoryInfo
{
    /// <summary>
    /// The category key to pass in the analytics request (e.g. "MATERIAL_CONSUMPTION").
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Human-readable display name (e.g. "Material Consumption").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Short description of what this category analyses.
    /// </summary>
    public required string Description { get; set; }
}
