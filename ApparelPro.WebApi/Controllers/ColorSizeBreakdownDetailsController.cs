using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.OrderManagement;
using ApparelPro.Data.Models.OrderManagement;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.OrderManagement;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/colorSizeBreakdownDetails")]
    [ApiController]
    public class ColorSizeBreakdownDetailsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IColorSizeBreakdownDetailsService _colorSizeBreakdownDetailsService;
        public ColorSizeBreakdownDetailsController(IMapper mapper, IColorSizeBreakdownDetailsService colorSizeDetailsService)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            if (colorSizeDetailsService == null)
            {
                throw new ArgumentNullException(nameof(colorSizeDetailsService));
            }
            _mapper = mapper;
            _colorSizeBreakdownDetailsService = colorSizeDetailsService;            
        }

        [HttpGet("list")]
        [Authorize(Roles = "Merchandiser,Merchandiser Manager")]
        [ProducesResponseType(typeof(PaginationAPIModel<ColorSizeBreakdownDetailsAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetColorSizeDetailsAsync(
           [FromQuery] int pageSize,
           [FromQuery] int pageNumber,
           [FromQuery] string? sortColumn = null,
           [FromQuery] string? sortOrder = null,
           [FromQuery] string? filterColumn = null,
           [FromQuery] string? filterQuery = null)
        {
            var colorSizeServiceModels = await _colorSizeBreakdownDetailsService.GetColorSizeDetailsAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var buyers = _mapper.Map<PaginationAPIModel<ColorSizeBreakdownDetailsAPIModel>>(colorSizeServiceModels);
            return Ok(buyers);
        }

        [HttpPost()]
        //[Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Bank Endpoints" },
         Summary = "Add a Bank.",
         Description = "Returns 200 - OK with No content")
     ]
        public async Task<IActionResult> AddColorSizeDetailsAsync([FromBody] CreateColorSizeBreakdownDetailsAPIModel  createColorSizeBreakdownDetailsAPIModel)
        {
            var createColorSizeDetailsServiceModel = _mapper.Map<CreateColorSizeBreakdownDetailsServiceModel>(createColorSizeBreakdownDetailsAPIModel);
            var addedColorSizeDetails = await _colorSizeBreakdownDetailsService.AddColorSizeDetailsAsync(createColorSizeDetailsServiceModel);
            return CreatedAtRoute(nameof(GetColorSizeDetailsAsync), new { buyerCode = addedColorSizeDetails.BuyerCode }, null);
        }

        [HttpGet("singleOrDefault-By-Style")]
        [Authorize(Roles = "Merchandiser,Merchandiser Manager")]
        [SwaggerOperation(
            Tags = new[] { "Order Management Matrix Endpoints" },
            Summary = "Retrieve Single color/size breakdown matrices for a style.",
            Description = "Retrieve unique single entity, Color Size breakdown details by buyer, order, type and style."
        )]
        [ProducesResponseType(typeof(List<ColorSizeBreakdownDetailsAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetBreakdownByStyleAsync(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            var colorSizeDetailsServiceModel  = await _colorSizeBreakdownDetailsService.GetBreakdownByStyleAsync(buyerCode, order, typeCode, styleCode);
           
            var mappedResult = _mapper.Map<List<ColorSizeBreakdownDetailsAPIModel>>(colorSizeDetailsServiceModel);
            return Ok(mappedResult);
        }

        [HttpPost("bulk-save")]
        [Authorize(Roles = "Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [SwaggerOperation(
            Tags = new[] { "Order Management Matrix Endpoints" },
            Summary = "Bulk saves size and colour breakdown matrices.",
            Description = "Purges historical records for the style scope and completes atomic insertion updates."
        )]
        public async Task<IActionResult> BulkSaveDetailsAsync(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode,
            [FromBody] List<CreateColorSizeBreakdownDetailsAPIModel> payload)
        {
            if (payload == null) return BadRequest("Matrix data payload can not be empty.");

            var serviceModels = _mapper.Map<List<CreateColorSizeBreakdownDetailsServiceModel>>(payload);

            await _colorSizeBreakdownDetailsService.BulkSaveColorSizeDetailsAsync(buyerCode, order, typeCode, styleCode, serviceModels);

            return Ok(new { Message = "Style allocation matrix synced with SQL Server successfully." });
        }

        [HttpGet("style-dimensions")]
        [ProducesResponseType(typeof(StyleDimensionsLookupAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetStyleDimensions(
          [FromQuery] int buyerCode,
          [FromQuery] string order,
          [FromQuery] int typeCode,
          [FromQuery] string styleCode)
        {
            try
            {
                var data = await _colorSizeBreakdownDetailsService.GetStyleDimensionsAsync(buyerCode, order, typeCode, styleCode);
                var styleDimensionsLookupAPIModel = _mapper.Map<StyleDimensionsLookupAPIModel>(data);
                return Ok(styleDimensionsLookupAPIModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to look up style colour/size dimensional metrics: {ex.Message}" });
            }
        }

        [HttpGet("color-size-matrix")]
        [ProducesResponseType(typeof(List<ColorSizeDetails>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetSavedColorSizeMatrix(
            [FromQuery] int buyerCode,
            [FromQuery] string order,
            [FromQuery] int typeCode,
            [FromQuery] string styleCode)
        {
            if (string.IsNullOrEmpty(order) || string.IsNullOrEmpty(styleCode))
                return BadRequest("Target order and style tracking parameters cannot be empty.");

            try
            {
                var matrixRows = await  _colorSizeBreakdownDetailsService.GetSavedColorSizeMatrixAsync(buyerCode, order, typeCode, styleCode);
                return Ok(matrixRows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to read saved color/size matrix records: {ex.Message}" });
            }
        }

    }
}
