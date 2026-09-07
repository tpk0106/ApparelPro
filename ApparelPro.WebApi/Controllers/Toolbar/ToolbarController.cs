using System.Security.Claims;
using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.Toolbar;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Toolbar
{
    [Route("api/toolbar")]
    [ApiController]
    [Authorize]
    public class ToolbarController : ControllerBase
    {
        private readonly IToolbarService _toolbarService;
        private readonly IMapper _mapper;

        public ToolbarController(IToolbarService toolbarService, IMapper mapper)
        {
            _toolbarService = toolbarService;
            _mapper = mapper;
        }

        // ClaimTypes.Name carries the authenticated user's email (see
        // StyleDetailsController) - the only identity claim this app issues,
        // used here to scope each user's own pin selections.
        private string CurrentUserEmail => User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        [HttpGet("preferences")]
        [ProducesResponseType(typeof(ToolbarPreferenceAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetPreferencesAsync()
        {
            var serviceModel = await _toolbarService.GetPreferencesAsync(CurrentUserEmail);
            return Ok(_mapper.Map<ToolbarPreferenceAPIModel>(serviceModel));
        }

        [HttpPut("preferences")]
        [ProducesResponseType(typeof(ToolbarPreferenceAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> SavePreferencesAsync([FromBody] SaveToolbarPreferenceAPIModel saveToolbarPreferenceAPIModel)
        {
            var saveServiceModel = _mapper.Map<apparelPro.BusinessLogic.Services.Models.Toolbar.IToolbarService.SaveToolbarPreferenceServiceModel>(saveToolbarPreferenceAPIModel);
            var serviceModel = await _toolbarService.SavePreferencesAsync(CurrentUserEmail, saveServiceModel);
            return Ok(_mapper.Map<ToolbarPreferenceAPIModel>(serviceModel));
        }
    }
}
