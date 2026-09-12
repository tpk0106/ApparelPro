using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IAgreementCodeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/agreement-code")]
    [ApiController]
    public class AgreementCodeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAgreementCodeService _agreementCodeService;
        public AgreementCodeController(IMapper mapper, IAgreementCodeService agreementCodeService)
        {
            _mapper = mapper;
            _agreementCodeService = agreementCodeService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "agreement-code-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(PaginationAPIModel<AgreementCodeAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Agreement Code Endpoints" },
           Summary = "Agreement Code list.",
           Description = "Returns 200 - OK with list")
       ]
        public async Task<IActionResult> GetAgreementCodesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
        )
        {
            var agreementCodeServiceModels = await _agreementCodeService.GetAgreementCodesAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var agreementCodes = _mapper.Map<PaginationAPIModel<AgreementCodeAPIModel>>(agreementCodeServiceModels);
            return Ok(agreementCodes);
        }

        [HttpPost()]
        [Authorize(Policy = "agreement-code-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Agreement Code Endpoints" },
           Summary = "Add an Agreement Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddAgreementCodeAsync([FromBody] CreateAgreementCodeAPIModel createAgreementCodeAPIModel)
        {
            try
            {
                var agreementCode = await _agreementCodeService.GetAgreementCodeByCodeAsync(createAgreementCodeAPIModel.Code);
                if (agreementCode != null)
                {
                    return BadRequest(new { message = "Agreement Code already exists" });
                }
                var createAgreementCodeServiceModel = _mapper.Map<CreateAgreementCodeServiceModel>(createAgreementCodeAPIModel);

                var addedAgreementCode = await _agreementCodeService.AddAgreementCodeAsync(createAgreementCodeServiceModel);
                var agreementCodeAPIModel = _mapper.Map<CreateAgreementCodeAPIModel>(addedAgreementCode);
                return CreatedAtRoute(nameof(GetAgreementCodeByCodeAsync), new { code = agreementCodeAPIModel.Code }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("list/{code}", Name = "GetAgreementCodeByCodeAsync")]
        [Authorize(Policy = "agreement-code-view")]
        [ProducesResponseType(typeof(AgreementCodeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Agreement Code Endpoints" },
             Summary = "Agreement Code details for a given Agreement Code Code",
             Description = "Returns 200 - OK with Agreement Code model.")
         ]
        public async Task<IActionResult> GetAgreementCodeByCodeAsync(string code)
        {
            try
            {
                var agreementCodeAPIModel = _mapper.Map<AgreementCodeAPIModel>(await _agreementCodeService.GetAgreementCodeByCodeAsync(code));
                if (agreementCodeAPIModel == null)
                {
                    return UnprocessableEntity("Agreement Code is not available for code :" + code);
                }
                return Ok(agreementCodeAPIModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "agreement-code-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Agreement Code Endpoints" },
           Summary = "Delete an Agreement Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeleteAgreementCodeAsync(string code)
        {
            var agreementCode = await _agreementCodeService.GetAgreementCodeByCodeAsync(code);
            if (agreementCode == null)
            {
                return UnprocessableEntity("Agreement Code is not available for code :" + code);
            }
            await _agreementCodeService.DeleteAgreementCodeAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Policy = "agreement-code-manage")]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Agreement Code Endpoints" },
           Summary = "Update an Agreement Code.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> UpdateAgreementCodeAsync([FromQuery] string code, [FromBody] UpdateAgreementCodeAPIModel
           updateAgreementCodeAPIModel)
        {
            try
            {
                var resultAgreementCodeAPIModel = _mapper.Map<AgreementCodeAPIModel>(await _agreementCodeService.GetAgreementCodeByCodeAsync(code));
                if (resultAgreementCodeAPIModel == null)
                {
                    return UnprocessableEntity("agreement code is not available for code :" + code);
                }
                updateAgreementCodeAPIModel.Code = resultAgreementCodeAPIModel.Code;
                var updateAgreementCodeServiceModel = _mapper.Map<UpdateAgreementCodeServiceModel>(updateAgreementCodeAPIModel);
                await _agreementCodeService.UpdateAgreementCodeAsync(updateAgreementCodeServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
