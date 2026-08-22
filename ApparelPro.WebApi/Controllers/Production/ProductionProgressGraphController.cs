using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    // Production Control -> Production Progress Graph - migrated from PR_PROG.PRG.
    [Route("api/production-progress-graph")]
    [ApiController]
    public class ProductionProgressGraphController : ControllerBase
    {
        private readonly IProductionProgressGraphService _productionProgressGraphService;
        private readonly IMapper _mapper;

        public ProductionProgressGraphController(
            IProductionProgressGraphService productionProgressGraphService, IMapper mapper)
        {
            _productionProgressGraphService = productionProgressGraphService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "production-progress-graph-view")]
        [ProducesResponseType(typeof(ProductionProgressGraphAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAsync(
            [FromQuery] int buyerCode, [FromQuery] string order, [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            try
            {
                var graph = await _productionProgressGraphService.GetGraphAsync(buyerCode, order, typeCode, styleCode);
                return Ok(_mapper.Map<ProductionProgressGraphAPIModel>(graph));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
        }
    }
}
