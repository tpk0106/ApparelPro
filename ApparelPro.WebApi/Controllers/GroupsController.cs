using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.Registration;
using ApparelPro.WebApi.Authorization;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/groups")]
    [ApiController]
    // Raw Roles check, not a dynamic RolePermissions-table policy key - deliberately
    // matching PermissionsController's own pattern. Gating "who can manage groups and
    // permissions" behind a permission that itself lives in the table being managed here
    // would be circular and a potential lockout footgun.
    [Authorize(Roles = AccessPolicies.AdministratorOnly)]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly IMapper _mapper;

        public GroupsController(IGroupService groupService, IMapper mapper)
        {
            _groupService = groupService;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<GroupAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetGroupsAsync()
        {
            var serviceModels = await _groupService.GetGroupsAsync();
            var apiModels = _mapper.Map<List<GroupAPIModel>>(serviceModels);
            return Ok(apiModels);
        }

        [HttpPost]
        [ProducesResponseType(typeof(GroupAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> CreateGroupAsync([FromBody] CreateGroupAPIModel createGroupAPIModel)
        {
            if (createGroupAPIModel == null || string.IsNullOrWhiteSpace(createGroupAPIModel.Name))
            {
                return BadRequest(new { message = "Group name is required." });
            }

            try
            {
                var serviceModel = await _groupService.CreateGroupAsync(createGroupAPIModel.Name);
                var apiModel = _mapper.Map<GroupAPIModel>(serviceModel);
                return Ok(apiModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(OkResult), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> DeleteGroupAsync(string id)
        {
            try
            {
                await _groupService.DeleteGroupAsync(id);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
