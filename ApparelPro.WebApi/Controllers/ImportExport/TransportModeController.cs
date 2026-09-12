using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ITransportModeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/transport-mode")]
    [ApiController]
    public class TransportModeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ITransportModeService _transportModeService;
        public TransportModeController(IMapper mapper, ITransportModeService transportModeService)
        {
            _mapper = mapper;
            _transportModeService = transportModeService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "transport-mode-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(PaginationAPIModel<TransportModeAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Transport Mode Endpoints" },
           Summary = "Transport Mode list.",
           Description = "Returns 200 - OK with list")
       ]
        public async Task<IActionResult> GetTransportModesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
        )
        {
            var transportModeServiceModels = await _transportModeService.GetTransportModesAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var transportModes = _mapper.Map<PaginationAPIModel<TransportModeAPIModel>>(transportModeServiceModels);
            return Ok(transportModes);
        }

        [HttpPost()]
        [Authorize(Policy = "transport-mode-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Transport Mode Endpoints" },
           Summary = "Add a Transport Mode.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddTransportModeAsync([FromBody] CreateTransportModeAPIModel createTransportModeAPIModel)
        {
            try
            {
                var transportMode = await _transportModeService.GetTransportModeByCodeAsync(createTransportModeAPIModel.Code);
                if (transportMode != null)
                {
                    return BadRequest(new { message = "Transport Mode already exists" });
                }
                var createTransportModeServiceModel = _mapper.Map<CreateTransportModeServiceModel>(createTransportModeAPIModel);

                var addedTransportMode = await _transportModeService.AddTransportModeAsync(createTransportModeServiceModel);
                var transportModeAPIModel = _mapper.Map<CreateTransportModeAPIModel>(addedTransportMode);
                return CreatedAtRoute(nameof(GetTransportModeByCodeAsync), new { code = transportModeAPIModel.Code }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("list/{code}", Name = "GetTransportModeByCodeAsync")]
        [Authorize(Policy = "transport-mode-view")]
        [ProducesResponseType(typeof(TransportModeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Transport Mode Endpoints" },
             Summary = "Transport Mode details for a given Transport Mode Code",
             Description = "Returns 200 - OK with Transport Mode model.")
         ]
        public async Task<IActionResult> GetTransportModeByCodeAsync(string code)
        {
            try
            {
                var transportModeAPIModel = _mapper.Map<TransportModeAPIModel>(await _transportModeService.GetTransportModeByCodeAsync(code));
                if (transportModeAPIModel == null)
                {
                    return UnprocessableEntity("Transport Mode is not available for code :" + code);
                }
                return Ok(transportModeAPIModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "transport-mode-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Transport Mode Endpoints" },
           Summary = "Delete a Transport Mode.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeleteTransportModeAsync(string code)
        {
            var transportMode = await _transportModeService.GetTransportModeByCodeAsync(code);
            if (transportMode == null)
            {
                return UnprocessableEntity("Transport Mode is not available for code :" + code);
            }
            await _transportModeService.DeleteTransportModeAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Policy = "transport-mode-manage")]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Transport Mode Endpoints" },
           Summary = "Update a Transport Mode.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> UpdateTransportModeAsync([FromQuery] string code, [FromBody] UpdateTransportModeAPIModel
           updateTransportModeAPIModel)
        {
            try
            {
                var resultTransportModeAPIModel = _mapper.Map<TransportModeAPIModel>(await _transportModeService.GetTransportModeByCodeAsync(code));
                if (resultTransportModeAPIModel == null)
                {
                    return UnprocessableEntity("transport mode is not available for code :" + code);
                }
                updateTransportModeAPIModel.Code = resultTransportModeAPIModel.Code;
                var updateTransportModeServiceModel = _mapper.Map<UpdateTransportModeServiceModel>(updateTransportModeAPIModel);
                await _transportModeService.UpdateTransportModeAsync(updateTransportModeServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
