using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.OrderwiseInventory
{
    [Route("api/orderwise-inventory-ain")]
    [ApiController]
    [Authorize(Policy = "ain")]
    public class AINController : ControllerBase
    {
        private readonly IAdditionalIssueNoteService _additionalIssueNoteService;
        private readonly IMapper _mapper;

        public AINController(IAdditionalIssueNoteService additionalIssueNoteService, IMapper mapper)
        {
            _additionalIssueNoteService = additionalIssueNoteService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-ain/issuable-stock?buyerCode=2&order=1017-18
        // Powers the item picker - one row per (Store, Item) for this Buyer/Order that
        // also has a Material Consumption profile.
        [HttpGet("issuable-stock")]
        public async Task<IActionResult> GetIssuableStock(
            [FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var result = await _additionalIssueNoteService.GetIssuableStockByBuyerOrderAsync(buyerCode, order);
                var resultAPIModel = _mapper.Map<List<AinIssuableStockRowAPIModel>>(result);
                return Ok(resultAPIModel);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load issuable stock: {ex.Message}" });
            }
        }

        // 2. POST: api/orderwise-inventory-ain/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitAdditionalIssue([FromBody] AinAPIModel ainAPIModel)
        {
            if (ainAPIModel == null)
                return BadRequest("The inbound Additional Issue Note payload cannot be empty.");

            if (ainAPIModel.Header == null || ainAPIModel.Lines == null || ainAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing critical document header or line items data.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<AinHeaderServiceModel>(ainAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<AinLineItemServiceModel>>(ainAPIModel.Lines);

            try
            {
                var success = await _additionalIssueNoteService.CommitAdditionalIssueNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Additional Issue Note for Order '{ainAPIModel.Header.Order}' committed and inventory balances updated successfully.",
                    AinNumber = headerServiceModel.AinNumber
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Transaction processing failed on the SQL server: {ex.Message}" });
            }
        }
    }
}
