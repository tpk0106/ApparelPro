using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICommodityCodeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/commodity-code")]
    [ApiController]
    public class CommodityCodeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICommodityCodeService _commodityCodeService;
        public CommodityCodeController(IMapper mapper, ICommodityCodeService commodityCodeService)
        {
            _mapper = mapper;
            _commodityCodeService = commodityCodeService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "commodity-code-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(PaginationAPIModel<CommodityCodeAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Commodity Code Endpoints" },
           Summary = "Commodity Code list.",
           Description = "Returns 200 - OK with list")
       ]
        public async Task<IActionResult> GetCommodityCodesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
        )
        {
            var commodityCodeServiceModels = await _commodityCodeService.GetCommodityCodesAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var commodityCodes = _mapper.Map<PaginationAPIModel<CommodityCodeAPIModel>>(commodityCodeServiceModels);
            return Ok(commodityCodes);
        }

        [HttpPost()]
        [Authorize(Policy = "commodity-code-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Commodity Code Endpoints" },
           Summary = "Add a Commodity Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddCommodityCodeAsync([FromBody] CreateCommodityCodeAPIModel createCommodityCodeAPIModel)
        {
            try
            {
                var commodityCode = await _commodityCodeService.GetCommodityCodeByCodeAsync(createCommodityCodeAPIModel.Code);
                if (commodityCode != null)
                {
                    return BadRequest(new { message = "Commodity Code already exists" });
                }
                var createCommodityCodeServiceModel = _mapper.Map<CreateCommodityCodeServiceModel>(createCommodityCodeAPIModel);

                var addedCommodityCode = await _commodityCodeService.AddCommodityCodeAsync(createCommodityCodeServiceModel);
                var commodityCodeAPIModel = _mapper.Map<CreateCommodityCodeAPIModel>(addedCommodityCode);
                return CreatedAtRoute(nameof(GetCommodityCodeByCodeAsync), new { code = commodityCodeAPIModel.Code }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("list/{code}", Name = "GetCommodityCodeByCodeAsync")]
        [Authorize(Policy = "commodity-code-view")]
        [ProducesResponseType(typeof(CommodityCodeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Commodity Code Endpoints" },
             Summary = "Commodity Code details for a given Commodity Code Code",
             Description = "Returns 200 - OK with Commodity Code model.")
         ]
        public async Task<IActionResult> GetCommodityCodeByCodeAsync(string code)
        {
            try
            {
                var commodityCodeAPIModel = _mapper.Map<CommodityCodeAPIModel>(await _commodityCodeService.GetCommodityCodeByCodeAsync(code));
                if (commodityCodeAPIModel == null)
                {
                    return UnprocessableEntity("Commodity Code is not available for code :" + code);
                }
                return Ok(commodityCodeAPIModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "commodity-code-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Commodity Code Endpoints" },
           Summary = "Delete a Commodity Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeleteCommodityCodeAsync(string code)
        {
            var commodityCode = await _commodityCodeService.GetCommodityCodeByCodeAsync(code);
            if (commodityCode == null)
            {
                return UnprocessableEntity("Commodity Code is not available for code :" + code);
            }
            await _commodityCodeService.DeleteCommodityCodeAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Policy = "commodity-code-manage")]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Commodity Code Endpoints" },
           Summary = "Update a Commodity Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> UpdateCommodityCodeAsync([FromQuery] string code, [FromBody] UpdateCommodityCodeAPIModel
           updateCommodityCodeAPIModel)
        {
            try
            {
                var resultCommodityCodeAPIModel = _mapper.Map<CommodityCodeAPIModel>(await _commodityCodeService.GetCommodityCodeByCodeAsync(code));
                if (resultCommodityCodeAPIModel == null)
                {
                    return UnprocessableEntity("commodity code is not available for code :" + code);
                }
                updateCommodityCodeAPIModel.Code = resultCommodityCodeAPIModel.Code;
                var updateCommodityCodeServiceModel = _mapper.Map<UpdateCommodityCodeServiceModel>(updateCommodityCodeAPIModel);
                await _commodityCodeService.UpdateCommodityCodeAsync(updateCommodityCodeServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
