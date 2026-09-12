using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ITaxBaseCodeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/tax-base-code")]
    [ApiController]
    public class TaxBaseCodeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ITaxBaseCodeService _taxBaseCodeService;
        public TaxBaseCodeController(IMapper mapper, ITaxBaseCodeService taxBaseCodeService)
        {
            _mapper = mapper;
            _taxBaseCodeService = taxBaseCodeService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "tax-base-code-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(PaginationAPIModel<TaxBaseCodeAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Tax Base Code Endpoints" },
           Summary = "Tax Base Code list.",
           Description = "Returns 200 - OK with list")
       ]
        public async Task<IActionResult> GetTaxBaseCodesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
        )
        {
            var taxBaseCodeServiceModels = await _taxBaseCodeService.GetTaxBaseCodesAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var taxBaseCodes = _mapper.Map<PaginationAPIModel<TaxBaseCodeAPIModel>>(taxBaseCodeServiceModels);
            return Ok(taxBaseCodes);
        }

        [HttpPost()]
        [Authorize(Policy = "tax-base-code-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Tax Base Code Endpoints" },
           Summary = "Add a Tax Base Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddTaxBaseCodeAsync([FromBody] CreateTaxBaseCodeAPIModel createTaxBaseCodeAPIModel)
        {
            try
            {
                var taxBaseCode = await _taxBaseCodeService.GetTaxBaseCodeByCodeAsync(createTaxBaseCodeAPIModel.Code);
                if (taxBaseCode != null)
                {
                    return BadRequest(new { message = "Tax Base Code already exists" });
                }
                var createTaxBaseCodeServiceModel = _mapper.Map<CreateTaxBaseCodeServiceModel>(createTaxBaseCodeAPIModel);

                var addedTaxBaseCode = await _taxBaseCodeService.AddTaxBaseCodeAsync(createTaxBaseCodeServiceModel);
                var taxBaseCodeAPIModel = _mapper.Map<CreateTaxBaseCodeAPIModel>(addedTaxBaseCode);
                return CreatedAtRoute(nameof(GetTaxBaseCodeByCodeAsync), new { code = taxBaseCodeAPIModel.Code }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("list/{code}", Name = "GetTaxBaseCodeByCodeAsync")]
        [Authorize(Policy = "tax-base-code-view")]
        [ProducesResponseType(typeof(TaxBaseCodeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Tax Base Code Endpoints" },
             Summary = "Tax Base Code details for a given Tax Base Code Code",
             Description = "Returns 200 - OK with Tax Base Code model.")
         ]
        public async Task<IActionResult> GetTaxBaseCodeByCodeAsync(string code)
        {
            try
            {
                var taxBaseCodeAPIModel = _mapper.Map<TaxBaseCodeAPIModel>(await _taxBaseCodeService.GetTaxBaseCodeByCodeAsync(code));
                if (taxBaseCodeAPIModel == null)
                {
                    return UnprocessableEntity("Tax Base Code is not available for code :" + code);
                }
                return Ok(taxBaseCodeAPIModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "tax-base-code-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Tax Base Code Endpoints" },
           Summary = "Delete a Tax Base Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeleteTaxBaseCodeAsync(string code)
        {
            var taxBaseCode = await _taxBaseCodeService.GetTaxBaseCodeByCodeAsync(code);
            if (taxBaseCode == null)
            {
                return UnprocessableEntity("Tax Base Code is not available for code :" + code);
            }
            await _taxBaseCodeService.DeleteTaxBaseCodeAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Policy = "tax-base-code-manage")]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Tax Base Code Endpoints" },
           Summary = "Update a Tax Base Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> UpdateTaxBaseCodeAsync([FromQuery] string code, [FromBody] UpdateTaxBaseCodeAPIModel
           updateTaxBaseCodeAPIModel)
        {
            try
            {
                var resultTaxBaseCodeAPIModel = _mapper.Map<TaxBaseCodeAPIModel>(await _taxBaseCodeService.GetTaxBaseCodeByCodeAsync(code));
                if (resultTaxBaseCodeAPIModel == null)
                {
                    return UnprocessableEntity("tax base code is not available for code :" + code);
                }
                updateTaxBaseCodeAPIModel.Code = resultTaxBaseCodeAPIModel.Code;
                var updateTaxBaseCodeServiceModel = _mapper.Map<UpdateTaxBaseCodeServiceModel>(updateTaxBaseCodeAPIModel);
                await _taxBaseCodeService.UpdateTaxBaseCodeAsync(updateTaxBaseCodeServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
