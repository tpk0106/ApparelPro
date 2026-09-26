using System.Text.Json;
using apparelPro.BusinessLogic.Services;
using ApparelPro.AI.Abstractions;
using ApparelPro.AI.Models;
using ApparelPro.WebApi.Misc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/ai")]
    [ApiController]
    [Authorize(Policy = "style-details")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly IStyleDetailsService _styleDetailsService;
        private readonly IMaterialConsumptionService _materialConsumptionService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierService _supplierService;
        private readonly IBuyerService _buyerService;

        public AiController(
            IAiService aiService,
            IStyleDetailsService styleDetailsService,
            IMaterialConsumptionService materialConsumptionService,
            IPurchaseOrderService purchaseOrderService,
            ISupplierService supplierService, IBuyerService buyerService)
        {
            _aiService = aiService;
            _styleDetailsService = styleDetailsService;
            _materialConsumptionService = materialConsumptionService;
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _buyerService = buyerService;
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Summarise any ApparelPro entity using AI.
        /// EntityKey format:
        ///   Style: "buyerCode/order/typeCode/styleCode" (e.g. "1/ORD001/2/ST001")
        ///   PurchaseOrder: "buyerCode/order" (e.g. "1/ORD001")
        ///   Supplier: "supplierCode" (e.g. "101")
        /// </summary>
        [HttpPost("summarise")]
        [ProducesResponseType(typeof(AiSummariseAPIModel_Response), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        [ProducesResponseType(HttpStatusCodes.InternalServerError)]
        public async Task<IActionResult> SummariseEntityAsync(
            [FromBody] AiSummariseAPIModel request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.EntityType) ||
                string.IsNullOrWhiteSpace(request.EntityKey))
            {
                return BadRequest("EntityType and EntityKey are required.");
            }

            string entityData;

            try
            {
                entityData = await ResolveEntityDataAsync(
                    request.EntityType.Trim(),
                    request.EntityKey.Trim(),
                    cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            AiCompletionResponse response;
            try
            {
                response = await _aiService.SummariseEntityAsync(
                    request.EntityType,
                    entityData,
                    request.UserQuery,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"AI provider request failed: {ex.Message}");
            }

            var result = new AiSummariseAPIModel_Response
            {
                Summary = response.Content,
                EntityType = request.EntityType,
                EntityKey = request.EntityKey,
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

        /// <summary>
        /// Deep analysis of any ApparelPro entity using AI (~1,500 words).
        /// Returns actionable insights, risk flags, and prioritised recommendations.
        /// Uses the same EntityKey format as the summarise endpoint.
        /// </summary>
        [HttpPost("analyse")]
        [ProducesResponseType(typeof(AiAnalyseAPIModel_Response), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        [ProducesResponseType(HttpStatusCodes.InternalServerError)]
        public async Task<IActionResult> AnalyseEntityAsync(
            [FromBody] AiSummariseAPIModel request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.EntityType) ||
                string.IsNullOrWhiteSpace(request.EntityKey))
            {
                return BadRequest("EntityType and EntityKey are required.");
            }

            string entityData;

            try
            {
                entityData = await ResolveEntityDataAsync(
                    request.EntityType.Trim(),
                    request.EntityKey.Trim(),
                    cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            var response = await _aiService.AnalyseEntityAsync(
                request.EntityType,
                entityData,
                request.UserQuery,
                cancellationToken);

            var result = new AiAnalyseAPIModel_Response
            {
                Analysis = response.Content,
                EntityType = request.EntityType,
                EntityKey = request.EntityKey,
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

        /// <summary>
        /// Resolves the entity key into serialised JSON data by calling
        /// the appropriate existing service.
        /// </summary>
        private async Task<string> ResolveEntityDataAsync(
            string entityType,
            string entityKey,
            CancellationToken cancellationToken)
        {
            switch (entityType.ToUpperInvariant())
            {
                case "STYLE":
                {
                    // Expected key format: "buyerCode/order/typeCode/styleCode"
                    var parts = entityKey.Split('/');
                    if (parts.Length != 4 ||
                        !int.TryParse(parts[0], out var buyerCode) ||
                        !int.TryParse(parts[2], out var typeCode))
                    {
                        throw new ArgumentException(
                            "Style EntityKey must be 'buyerCode/order/typeCode/styleCode' " +
                            "(e.g. '1/ORD001/2/ST001').");
                    }

                    var style = await _styleDetailsService
                        .GetStyleDetailsByBuyerOrderTypeStyleAsync(
                            buyerCode, parts[1], typeCode, parts[3]);

                    if (style == null)
                        throw new KeyNotFoundException(
                            $"Style not found: {entityKey}");

                        // Enrich with buyer name
                        var buyer = await _buyerService.GetBuyerByBuyerCodeAsync(buyerCode);

                        // Also fetch the consumption ledger for this style
                        var ledger = await _materialConsumptionService
                        .GetLedgerEntriesByStyleAsync(
                            buyerCode, parts[1], typeCode, parts[3]);

                        var combined = new
                        {
                            StyleDetails = style,
                            BuyerName = buyer?.Name,
                            MaterialConsumptionLedger = ledger,
                            ConsumptionLineCount = ledger?.Count ?? 0
                        };
                        //var combined = new
                        //{
                        //    StyleDetails = style,
                        //    MaterialConsumptionLedger = ledger,
                        //    ConsumptionLineCount = ledger?.Count ?? 0
                        //};

                        return JsonSerializer.Serialize(combined, JsonOptions);
                }

                case "PURCHASEORDER":
                case "PO":
                {
                    // Expected key format: "buyerCode/order"
                    var parts = entityKey.Split('/');
                    if (parts.Length != 2 ||
                        !int.TryParse(parts[0], out var buyerCode))
                    {
                        throw new ArgumentException(
                            "PurchaseOrder EntityKey must be 'buyerCode/order' " +
                            "(e.g. '1/ORD001').");
                    }

                    var po = await _purchaseOrderService
                        .GetPurchaseOrderByBuyerAndOrderAsync(buyerCode, parts[1]);

                    if (po == null)
                        throw new KeyNotFoundException(
                            $"Purchase Order not found: {entityKey}");

                    return JsonSerializer.Serialize(po, JsonOptions);
                }

                case "SUPPLIER":
                {
                    if (!int.TryParse(entityKey, out var supplierCode))
                    {
                        throw new ArgumentException(
                            "Supplier EntityKey must be a numeric supplier code " +
                            "(e.g. '101').");
                    }

                    var supplier = await _supplierService
                        .GetSupplierBySupplierCodeAsync(supplierCode);

                    if (supplier == null)
                        throw new KeyNotFoundException(
                            $"Supplier not found: {entityKey}");

                    return JsonSerializer.Serialize(supplier, JsonOptions);
                }

                default:
                    throw new ArgumentException(
                        $"Unsupported entity type: '{entityType}'. " +
                        "Supported types: Style, PurchaseOrder, Supplier.");
            }
        }
    }
}
