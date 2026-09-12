using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsProcedureCodeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/customs-procedure-code")]
    [ApiController]
    public class CustomsProcedureCodeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICustomsProcedureCodeService _customsProcedureCodeService;
        public CustomsProcedureCodeController(IMapper mapper, ICustomsProcedureCodeService customsProcedureCodeService)
        {
            _mapper = mapper;
            _customsProcedureCodeService = customsProcedureCodeService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "customs-procedure-code-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(PaginationAPIModel<CustomsProcedureCodeAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Customs Procedure Code Endpoints" },
           Summary = "Customs Procedure Code list.",
           Description = "Returns 200 - OK with list")
       ]
        public async Task<IActionResult> GetCustomsProcedureCodesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
        )
        {
            var customsProcedureCodeServiceModels = await _customsProcedureCodeService.GetCustomsProcedureCodesAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var customsProcedureCodes = _mapper.Map<PaginationAPIModel<CustomsProcedureCodeAPIModel>>(customsProcedureCodeServiceModels);
            return Ok(customsProcedureCodes);
        }

        [HttpPost()]
        [Authorize(Policy = "customs-procedure-code-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Customs Procedure Code Endpoints" },
           Summary = "Add a Customs Procedure Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddCustomsProcedureCodeAsync([FromBody] CreateCustomsProcedureCodeAPIModel createCustomsProcedureCodeAPIModel)
        {
            try
            {
                var customsProcedureCode = await _customsProcedureCodeService.GetCustomsProcedureCodeByCodeAsync(createCustomsProcedureCodeAPIModel.Code);
                if (customsProcedureCode != null)
                {
                    return BadRequest(new { message = "Customs Procedure Code already exists" });
                }
                var createCustomsProcedureCodeServiceModel = _mapper.Map<CreateCustomsProcedureCodeServiceModel>(createCustomsProcedureCodeAPIModel);

                var addedCustomsProcedureCode = await _customsProcedureCodeService.AddCustomsProcedureCodeAsync(createCustomsProcedureCodeServiceModel);
                var customsProcedureCodeAPIModel = _mapper.Map<CreateCustomsProcedureCodeAPIModel>(addedCustomsProcedureCode);
                return CreatedAtRoute(nameof(GetCustomsProcedureCodeByCodeAsync), new { code = customsProcedureCodeAPIModel.Code }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("list/{code}", Name = "GetCustomsProcedureCodeByCodeAsync")]
        [Authorize(Policy = "customs-procedure-code-view")]
        [ProducesResponseType(typeof(CustomsProcedureCodeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Customs Procedure Code Endpoints" },
             Summary = "Customs Procedure Code details for a given Customs Procedure Code Code",
             Description = "Returns 200 - OK with Customs Procedure Code model.")
         ]
        public async Task<IActionResult> GetCustomsProcedureCodeByCodeAsync(string code)
        {
            try
            {
                var customsProcedureCodeAPIModel = _mapper.Map<CustomsProcedureCodeAPIModel>(await _customsProcedureCodeService.GetCustomsProcedureCodeByCodeAsync(code));
                if (customsProcedureCodeAPIModel == null)
                {
                    return UnprocessableEntity("Customs Procedure Code is not available for code :" + code);
                }
                return Ok(customsProcedureCodeAPIModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "customs-procedure-code-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Customs Procedure Code Endpoints" },
           Summary = "Delete a Customs Procedure Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeleteCustomsProcedureCodeAsync(string code)
        {
            var customsProcedureCode = await _customsProcedureCodeService.GetCustomsProcedureCodeByCodeAsync(code);
            if (customsProcedureCode == null)
            {
                return UnprocessableEntity("Customs Procedure Code is not available for code :" + code);
            }
            await _customsProcedureCodeService.DeleteCustomsProcedureCodeAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Policy = "customs-procedure-code-manage")]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Customs Procedure Code Endpoints" },
           Summary = "Update a Customs Procedure Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> UpdateCustomsProcedureCodeAsync([FromQuery] string code, [FromBody] UpdateCustomsProcedureCodeAPIModel
           updateCustomsProcedureCodeAPIModel)
        {
            try
            {
                var resultCustomsProcedureCodeAPIModel = _mapper.Map<CustomsProcedureCodeAPIModel>(await _customsProcedureCodeService.GetCustomsProcedureCodeByCodeAsync(code));
                if (resultCustomsProcedureCodeAPIModel == null)
                {
                    return UnprocessableEntity("customs procedure code is not available for code :" + code);
                }
                updateCustomsProcedureCodeAPIModel.Code = resultCustomsProcedureCodeAPIModel.Code;
                var updateCustomsProcedureCodeServiceModel = _mapper.Map<UpdateCustomsProcedureCodeServiceModel>(updateCustomsProcedureCodeAPIModel);
                await _customsProcedureCodeService.UpdateCustomsProcedureCodeAsync(updateCustomsProcedureCodeServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
