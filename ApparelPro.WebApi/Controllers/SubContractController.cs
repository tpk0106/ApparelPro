using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.OrderManagement.ISubContractService;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    // Replicates od_subc1.prg ("D. Sub Contracts" - Order Management -> Sub Contracts in
    // legacy OD_MENU.PRG). Per-style Sub Contract allocations - see SubContract.cs for the
    // full class-level history/decision notes (2026-08-09).
    [Route("api/sub-contract")]
    [ApiController]
    public class SubContractController : ControllerBase
    {
        private readonly ISubContractService _subContractService;
        private readonly IMapper _mapper;

        public SubContractController(ISubContractService subContractService, IMapper mapper)
        {
            _subContractService = subContractService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "sub-contract-view")]
        [ProducesResponseType(typeof(List<SubContractAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetSubContractsAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            if (string.IsNullOrWhiteSpace(order) || string.IsNullOrWhiteSpace(styleCode))
                return BadRequest("Buyer, Order, Type, and Style are all required.");

            var result = await _subContractService.GetSubContractsAsync(buyerCode, order, typeCode, styleCode);
            return Ok(_mapper.Map<List<SubContractAPIModel>>(result));
        }

        [HttpPost]
        [Authorize(Policy = "sub-contract-manage")]
        [ProducesResponseType(typeof(SaveSubContractResultAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SaveSubContractAsync([FromBody] SaveSubContractAPIModel request)
        {
            if (request == null) return BadRequest("Entry request data payload cannot be empty.");

            try
            {
                var serviceModel = _mapper.Map<SaveSubContractServiceModel>(request);
                var result = await _subContractService.SaveSubContractAsync(serviceModel);
                return Ok(_mapper.Map<SaveSubContractResultAPIModel>(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to save Sub Contract entry: {ex.Message}" });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "sub-contract-manage")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> DeleteSubContractAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode,
            [FromQuery] string subContractorCode)
        {
            try
            {
                var success = await _subContractService.DeleteSubContractAsync(buyerCode, order, typeCode, styleCode, subContractorCode);
                if (!success)
                    return NotFound(new { Message = "Sub Contract entry not found." });

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Deletion Failed: {ex.Message}" });
            }
        }
    }
}
