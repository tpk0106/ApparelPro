using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IProductionLineService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/production-line")]
    [ApiController]
    public class ProductionLineController : ControllerBase
    {
        private readonly IProductionLineService _productionLineService;
        private readonly IMapper _mapper;

        public ProductionLineController(IProductionLineService productionLineService, IMapper mapper)
        {
            _productionLineService = productionLineService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "production-line-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<ProductionLineAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetProductionLinesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var serviceModels = await _productionLineService.GetProductionLinesAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var itemsPage = _mapper.Map<PaginationAPIModel<ProductionLineAPIModel>>(serviceModels);
            return Ok(itemsPage);
        }

        [HttpGet("list/{lineCode}", Name = "GetProductionLineByLineCodeAsync")]
        [Authorize(Policy = "production-line-view")]
        [ProducesResponseType(typeof(ProductionLineAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetProductionLineByLineCodeAsync(string lineCode)
        {
            var productionLine = await _productionLineService.GetProductionLineByLineCodeAsync(lineCode);
            if (productionLine == null)
            {
                return UnprocessableEntity("Production line is not available for code :" + lineCode);
            }
            return Ok(_mapper.Map<ProductionLineAPIModel>(productionLine));
        }

        [HttpPost]
        [Authorize(Policy = "production-line-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddProductionLineAsync([FromBody] CreateProductionLineAPIModel createAPIModel)
        {
            var createServiceModel = _mapper.Map<CreateProductionLineServiceModel>(createAPIModel);
            var added = await _productionLineService.AddProductionLineAsync(createServiceModel);
            return CreatedAtRoute(nameof(GetProductionLineByLineCodeAsync), new { lineCode = added.LineCode }, null);
        }

        [HttpPut]
        [Authorize(Policy = "production-line-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateProductionLineAsync([FromQuery] string lineCode, [FromBody] UpdateProductionLineAPIModel updateAPIModel)
        {
            var existing = await _productionLineService.GetProductionLineByLineCodeAsync(lineCode);
            if (existing == null)
            {
                return UnprocessableEntity("Production line is not available for code :" + lineCode);
            }
            updateAPIModel.LineCode = lineCode;
            var updateServiceModel = _mapper.Map<UpdateProductionLineServiceModel>(updateAPIModel);
            await _productionLineService.UpdateProductionLineAsync(updateServiceModel);
            return NoContent();
        }

        [HttpDelete("{lineCode}")]
        [Authorize(Policy = "production-line-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteProductionLineAsync(string lineCode)
        {
            var existing = await _productionLineService.GetProductionLineByLineCodeAsync(lineCode);
            if (existing == null)
            {
                return UnprocessableEntity("Production line is not available for code :" + lineCode);
            }
            await _productionLineService.DeleteProductionLineAsync(lineCode);
            return NoContent();
        }
    }
}
