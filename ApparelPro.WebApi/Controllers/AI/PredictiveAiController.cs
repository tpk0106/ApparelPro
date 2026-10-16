using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Models;
using ApparelPro.WebApi.Misc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers;

// ─────────────────────────────────────────────────────────────────────────────
// 🎓 PREDICTIVE AI CONTROLLER
// Phase 5: REST endpoints for predictive & prescriptive AI.
//
// 🎓 WHY A SEPARATE CONTROLLER (not adding to CrossEntityAnalyticsController)?
// Single Responsibility Principle: CrossEntityAnalyticsController handles
// BACKWARD-LOOKING analytics ("What happened? What are the patterns?").
// This controller handles FORWARD-LOOKING predictions ("What will happen?
// What should we do about it?"). Different request shape (includes
// timeHorizonMonths), different service, different response structure
// (predictions vs analysis).
//
// Endpoints:
//   POST /api/ai/predictions         → Run a prediction for a category
//   GET  /api/ai/predictions/categories → List available prediction categories
//
// 🎓 AUTHORIZATION:
// Uses the same "style-details" policy as AiController and
// CrossEntityAnalyticsController — predictive data is derived from styles,
// orders, suppliers, and consumption. If stricter control is needed later,
// a dedicated "predictions" policy can be added.
// ─────────────────────────────────────────────────────────────────────────────

[Route("api/ai/predictions")]
[ApiController]
[Authorize(Policy = "style-details")]
public class PredictiveAiController : ControllerBase
{
    private readonly IPredictiveAiService _predictiveService;

    public PredictiveAiController(IPredictiveAiService predictiveService)
    {
        _predictiveService = predictiveService;
    }

    /// <summary>
    /// 🎓 Run a predictive analysis for the specified category.
    ///
    /// Supported categories (case-insensitive):
    ///   DEMAND_FORECASTING  — future order quantity predictions
    ///   MATERIAL_SHORTAGE   — materials at risk of shortage
    ///   SUPPLIER_RISK       — risk scores for each supplier
    ///   COST_OPTIMIZATION   — specific cost reduction recommendations
    ///
    /// Example request body:
    /// {
    ///   "category": "DEMAND_FORECASTING",
    ///   "userQuery": "What will buyer ALPHA's demand look like next quarter?",
    ///   "timeHorizonMonths": 3
    /// }
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PredictiveAiAPIModel_Response), HttpStatusCodes.OK)]
    [ProducesResponseType(HttpStatusCodes.BadRequest)]
    [ProducesResponseType(HttpStatusCodes.InternalServerError)]
    public async Task<IActionResult> RunPredictionAsync(
        [FromBody] PredictiveAiAPIModel request,
        CancellationToken cancellationToken)
    {
        // 🎓 VALIDATION: Category is the only required field
        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return BadRequest("Category is required. " +
                "Supported: DEMAND_FORECASTING, MATERIAL_SHORTAGE, " +
                "SUPPLIER_RISK, COST_OPTIMIZATION.");
        }

        // 🎓 VALIDATION: Time horizon must be 1-12 months (if provided)
        if (request.TimeHorizonMonths.HasValue &&
            (request.TimeHorizonMonths.Value < 1 || request.TimeHorizonMonths.Value > 12))
        {
            return BadRequest("TimeHorizonMonths must be between 1 and 12.");
        }

        try
        {
            // 🎓 The service handles everything:
            //   1. Validates the category
            //   2. Aggregates historical data from EF Core
            //   3. Computes trend signals (velocities, ratios, risk scores)
            //   4. Builds the prompt from PredictivePromptTemplates
            //   5. Calls IAiService.CompleteAsync()
            //   6. Returns (AiCompletionResponse, entityCount)
            var (response, entityCount) = await _predictiveService.GetPredictionAsync(
                request.Category.Trim(),
                request.UserQuery,
                cancellationToken);

            // 🎓 Determine the effective time horizon (default 3 months)
            var effectiveHorizon = request.TimeHorizonMonths.HasValue
                ? Math.Clamp(request.TimeHorizonMonths.Value, 1, 12)
                : 3;

            // 🎓 Map the service response to the API response model
            var result = new PredictiveAiAPIModel_Response
            {
                Prediction = response.Content,
                Category = request.Category.Trim().ToUpperInvariant(),
                EntityCount = entityCount,
                TimeHorizonMonths = effectiveHorizon,
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
            // 🎓 Thrown by GetPredictiveTemplate for unsupported categories
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            // 🎓 Thrown when the AI provider HTTP call fails
            return StatusCode(502, $"AI provider request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// 🎓 Returns the list of supported prediction categories.
    /// Useful for the frontend to populate a dropdown/selector in the
    /// Predictions Dashboard.
    ///
    /// GET /api/ai/predictions/categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<PredictionCategoryInfo>), HttpStatusCodes.OK)]
    public IActionResult GetCategories()
    {
        // 🎓 Static list — matches PredictivePromptTemplates.GetPredictiveTemplate switch
        var categories = new[]
        {
            new PredictionCategoryInfo
            {
                Key = "DEMAND_FORECASTING",
                Name = "Demand Forecasting",
                Description = "Predict future order quantities based on historical buyer patterns, seasonal trends, and style demand velocity",
                IconHint = "TrendingUp"
            },
            new PredictionCategoryInfo
            {
                Key = "MATERIAL_SHORTAGE",
                Name = "Material Shortage Predictor",
                Description = "Identify materials at risk of shortage by analysing planned consumption, supplier coverage, and sole-sourcing risks",
                IconHint = "WarningAmber"
            },
            new PredictionCategoryInfo
            {
                Key = "SUPPLIER_RISK",
                Name = "Supplier Risk Scoring",
                Description = "Score and rank suppliers by multi-dimensional risk: concentration, dependency, contact gaps, and revenue exposure",
                IconHint = "Shield"
            },
            new PredictionCategoryInfo
            {
                Key = "COST_OPTIMIZATION",
                Name = "Cost Optimisation Advisor",
                Description = "Quantify cost reduction opportunities from wastage, pricing inconsistencies, BOM complexity, and material efficiency",
                IconHint = "Savings"
            }
        };

        return Ok(categories);
    }
}
