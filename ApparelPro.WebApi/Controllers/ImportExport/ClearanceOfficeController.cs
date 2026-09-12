using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IClearanceOfficeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/clearance-office")]
    [ApiController]
    public class ClearanceOfficeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IClearanceOfficeService _clearanceOfficeService;
        public ClearanceOfficeController(IMapper mapper, IClearanceOfficeService clearanceOfficeService)
        {
            _mapper = mapper;
            _clearanceOfficeService = clearanceOfficeService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "clearance-office-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(PaginationAPIModel<ClearanceOfficeAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Clearance Office Endpoints" },
           Summary = "Clearance Office list.",
           Description = "Returns 200 - OK with list")
       ]
        public async Task<IActionResult> GetClearanceOfficesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
        )
        {
            var clearanceOfficeServiceModels = await _clearanceOfficeService.GetClearanceOfficesAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var clearanceOffices = _mapper.Map<PaginationAPIModel<ClearanceOfficeAPIModel>>(clearanceOfficeServiceModels);
            return Ok(clearanceOffices);
        }

        [HttpPost()]
        [Authorize(Policy = "clearance-office-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Clearance Office Endpoints" },
           Summary = "Add a Clearance Office.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddClearanceOfficeAsync([FromBody] CreateClearanceOfficeAPIModel createClearanceOfficeAPIModel)
        {
            try
            {
                var clearanceOffice = await _clearanceOfficeService.GetClearanceOfficeByCodeAsync(createClearanceOfficeAPIModel.Code);
                if (clearanceOffice != null)
                {
                    return BadRequest(new { message = "Clearance Office already exists" });
                }
                var createClearanceOfficeServiceModel = _mapper.Map<CreateClearanceOfficeServiceModel>(createClearanceOfficeAPIModel);

                var addedClearanceOffice = await _clearanceOfficeService.AddClearanceOfficeAsync(createClearanceOfficeServiceModel);
                var clearanceOfficeAPIModel = _mapper.Map<CreateClearanceOfficeAPIModel>(addedClearanceOffice);
                return CreatedAtRoute(nameof(GetClearanceOfficeByCodeAsync), new { code = clearanceOfficeAPIModel.Code }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("list/{code}", Name = "GetClearanceOfficeByCodeAsync")]
        [Authorize(Policy = "clearance-office-view")]
        [ProducesResponseType(typeof(ClearanceOfficeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Clearance Office Endpoints" },
             Summary = "Clearance Office details for a given Clearance Office Code",
             Description = "Returns 200 - OK with Clearance Office model.")
         ]
        public async Task<IActionResult> GetClearanceOfficeByCodeAsync(string code)
        {
            try
            {
                var clearanceOfficeAPIModel = _mapper.Map<ClearanceOfficeAPIModel>(await _clearanceOfficeService.GetClearanceOfficeByCodeAsync(code));
                if (clearanceOfficeAPIModel == null)
                {
                    return UnprocessableEntity("Clearance Office is not available for code :" + code);
                }
                return Ok(clearanceOfficeAPIModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "clearance-office-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Clearance Office Endpoints" },
           Summary = "Delete a Clearance Office.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeleteClearanceOfficeAsync(string code)
        {
            var clearanceOffice = await _clearanceOfficeService.GetClearanceOfficeByCodeAsync(code);
            if (clearanceOffice == null)
            {
                return UnprocessableEntity("Clearance Office is not available for code :" + code);
            }
            await _clearanceOfficeService.DeleteClearanceOfficeAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Policy = "clearance-office-manage")]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Clearance Office Endpoints" },
           Summary = "Update a Clearance Office.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> UpdateClearanceOfficeAsync([FromQuery] string code, [FromBody] UpdateClearanceOfficeAPIModel
           updateClearanceOfficeAPIModel)
        {
            try
            {
                var resultClearanceOfficeAPIModel = _mapper.Map<ClearanceOfficeAPIModel>(await _clearanceOfficeService.GetClearanceOfficeByCodeAsync(code));
                if (resultClearanceOfficeAPIModel == null)
                {
                    return UnprocessableEntity("clearance office is not available for code :" + code);
                }
                updateClearanceOfficeAPIModel.Code = resultClearanceOfficeAPIModel.Code;
                var updateClearanceOfficeServiceModel = _mapper.Map<UpdateClearanceOfficeServiceModel>(updateClearanceOfficeAPIModel);
                await _clearanceOfficeService.UpdateClearanceOfficeAsync(updateClearanceOfficeServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
