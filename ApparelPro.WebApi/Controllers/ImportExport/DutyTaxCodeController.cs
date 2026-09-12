using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IDutyTaxCodeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/duty-tax-code")]
    [ApiController]
    public class DutyTaxCodeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IDutyTaxCodeService _dutyTaxCodeService;
        public DutyTaxCodeController(IMapper mapper, IDutyTaxCodeService dutyTaxCodeService)
        {
            _mapper = mapper;
            _dutyTaxCodeService = dutyTaxCodeService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "duty-tax-code-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(PaginationAPIModel<DutyTaxCodeAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Duty Tax Code Endpoints" },
           Summary = "Duty Tax Code list.",
           Description = "Returns 200 - OK with list")
       ]
        public async Task<IActionResult> GetDutyTaxCodesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
        )
        {
            var dutyTaxCodeServiceModels = await _dutyTaxCodeService.GetDutyTaxCodesAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var dutyTaxCodes = _mapper.Map<PaginationAPIModel<DutyTaxCodeAPIModel>>(dutyTaxCodeServiceModels);
            return Ok(dutyTaxCodes);
        }

        [HttpPost()]
        [Authorize(Policy = "duty-tax-code-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Duty Tax Code Endpoints" },
           Summary = "Add a Duty Tax Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddDutyTaxCodeAsync([FromBody] CreateDutyTaxCodeAPIModel createDutyTaxCodeAPIModel)
        {
            try
            {
                var dutyTaxCode = await _dutyTaxCodeService.GetDutyTaxCodeByCodeAsync(createDutyTaxCodeAPIModel.Code);
                if (dutyTaxCode != null)
                {
                    return BadRequest(new { message = "Duty Tax Code already exists" });
                }
                var createDutyTaxCodeServiceModel = _mapper.Map<CreateDutyTaxCodeServiceModel>(createDutyTaxCodeAPIModel);

                var addedDutyTaxCode = await _dutyTaxCodeService.AddDutyTaxCodeAsync(createDutyTaxCodeServiceModel);
                var dutyTaxCodeAPIModel = _mapper.Map<CreateDutyTaxCodeAPIModel>(addedDutyTaxCode);
                return CreatedAtRoute(nameof(GetDutyTaxCodeByCodeAsync), new { code = dutyTaxCodeAPIModel.Code }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("list/{code}", Name = "GetDutyTaxCodeByCodeAsync")]
        [Authorize(Policy = "duty-tax-code-view")]
        [ProducesResponseType(typeof(DutyTaxCodeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Duty Tax Code Endpoints" },
             Summary = "Duty Tax Code details for a given Duty Tax Code Code",
             Description = "Returns 200 - OK with Duty Tax Code model.")
         ]
        public async Task<IActionResult> GetDutyTaxCodeByCodeAsync(string code)
        {
            try
            {
                var dutyTaxCodeAPIModel = _mapper.Map<DutyTaxCodeAPIModel>(await _dutyTaxCodeService.GetDutyTaxCodeByCodeAsync(code));
                if (dutyTaxCodeAPIModel == null)
                {
                    return UnprocessableEntity("Duty Tax Code is not available for code :" + code);
                }
                return Ok(dutyTaxCodeAPIModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "duty-tax-code-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Duty Tax Code Endpoints" },
           Summary = "Delete a Duty Tax Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeleteDutyTaxCodeAsync(string code)
        {
            var dutyTaxCode = await _dutyTaxCodeService.GetDutyTaxCodeByCodeAsync(code);
            if (dutyTaxCode == null)
            {
                return UnprocessableEntity("Duty Tax Code is not available for code :" + code);
            }
            await _dutyTaxCodeService.DeleteDutyTaxCodeAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Policy = "duty-tax-code-manage")]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Duty Tax Code Endpoints" },
           Summary = "Update a Duty Tax Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> UpdateDutyTaxCodeAsync([FromQuery] string code, [FromBody] UpdateDutyTaxCodeAPIModel
           updateDutyTaxCodeAPIModel)
        {
            try
            {
                var resultDutyTaxCodeAPIModel = _mapper.Map<DutyTaxCodeAPIModel>(await _dutyTaxCodeService.GetDutyTaxCodeByCodeAsync(code));
                if (resultDutyTaxCodeAPIModel == null)
                {
                    return UnprocessableEntity("duty tax code is not available for code :" + code);
                }
                updateDutyTaxCodeAPIModel.Code = resultDutyTaxCodeAPIModel.Code;
                var updateDutyTaxCodeServiceModel = _mapper.Map<UpdateDutyTaxCodeServiceModel>(updateDutyTaxCodeAPIModel);
                await _dutyTaxCodeService.UpdateDutyTaxCodeAsync(updateDutyTaxCodeServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
