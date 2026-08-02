using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.SystemConfiguration;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    // Any authenticated user can read the current settings (the Styles grid needs to know
    // whether override is on to decide how to warn); only an Administrator can change one.
    [Route("api/system-parameters")]
    [ApiController]
    [Authorize]
    public class SystemParametersController : ControllerBase
    {
        private readonly ISystemParameterService _systemParameterService;
        private readonly IMapper _mapper;

        public SystemParametersController(ISystemParameterService systemParameterService, IMapper mapper)
        {
            _systemParameterService = systemParameterService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<SystemParameterAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetSystemParametersAsync()
        {
            var parameters = await _systemParameterService.GetAllParametersAsync();
            return Ok(_mapper.Map<IEnumerable<SystemParameterAPIModel>>(parameters));
        }

        [HttpPut("{parameterKey}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateSystemParameterAsync(string parameterKey, [FromBody] UpdateSystemParameterAPIModel model)
        {
            try
            {
                await _systemParameterService.UpdateParameterAsync(parameterKey, model.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
