using apparelPro.BusinessLogic.Services.interfaces.OrderwiseInventory;
using apparelPro.BusinessLogic.Services.Models.OrderwiseInventory;
using ApparelPro.WebApi.APIModels.OrderwiseInventory;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/orderwise-inventory-dgn")]
    [ApiController]
    [Authorize(Roles = "Inventory, Merchandiser, Merchandiser Manager, Order Entry Operator")]
    public class DGNController : ControllerBase
    {
        private readonly IDamagedGoodsNoteService _damagedGoodsNoteService;
        private readonly IMapper _mapper;

        public DGNController(IDamagedGoodsNoteService damagedGoodsNoteService, IMapper mapper)
        {
            _damagedGoodsNoteService = damagedGoodsNoteService;
            _mapper = mapper;
        }

        // 1. GET: api/orderwise-inventory-dgn/damageable-stock?buyerCode=1&order=1017-18
        // Powers the item picker — one row per (Store, Item) that currently has stock on
        // hand and can be written off as damaged.
        [HttpGet("damageable-stock")]
        public async Task<IActionResult> GetDamageableStock(
            [FromQuery] int buyerCode, [FromQuery] string order)
        {
            if (string.IsNullOrWhiteSpace(order))
                return BadRequest("Parameter 'order' is required.");

            try
            {
                var result = await _damagedGoodsNoteService.GetDamageableStockByBuyerOrderAsync(buyerCode, order);
                var resultAPIModel = _mapper.Map<List<DgnDamageableStockRowAPIModel>>(result);
                return Ok(resultAPIModel);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to load damageable stock: {ex.Message}" });
            }
        }

        // 2. POST: api/orderwise-inventory-dgn/commit
        [HttpPost("commit")]
        public async Task<IActionResult> CommitDamagedGoods([FromBody] DgnAPIModel dgnAPIModel)
        {
            if (dgnAPIModel == null)
                return BadRequest("The inbound Damaged Goods Note payload cannot be empty.");

            if (dgnAPIModel.Header == null || dgnAPIModel.Lines == null || dgnAPIModel.Lines.Count == 0)
                return BadRequest("Validation Failure: Missing critical document header or line items data.");

            string activeUsername = User.Identity?.Name ?? "STORES CLERK";

            var headerServiceModel = _mapper.Map<DgnHeaderServiceModel>(dgnAPIModel.Header);
            var lineServiceModels = _mapper.Map<List<DgnLineItemServiceModel>>(dgnAPIModel.Lines);

            try
            {
                var success = await _damagedGoodsNoteService.CommitDamagedGoodsNoteAsync(
                    headerServiceModel,
                    lineServiceModels,
                    activeUsername);

                return Ok(new
                {
                    Success = success,
                    Message = $"Damaged Goods Note for Order '{dgnAPIModel.Header.Order}' committed and inventory balances updated successfully.",
                    DgnNumber = headerServiceModel.DgnNumber
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
