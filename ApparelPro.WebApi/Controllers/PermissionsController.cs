using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Registration.IPermissionService;
using ApparelPro.WebApi.APIModels.Registration;
using ApparelPro.WebApi.Authorization;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    [Authorize(Roles = AccessPolicies.AdministratorOnly)]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly IRolePermissionCache _rolePermissionCache;
        private readonly IMapper _mapper;

        public PermissionsController(IPermissionService permissionService, IRolePermissionCache rolePermissionCache, IMapper mapper)
        {
            _permissionService = permissionService;
            _rolePermissionCache = rolePermissionCache;
            _mapper = mapper;
        }

        [HttpGet("catalog")]
        [ProducesResponseType(typeof(List<PermissionAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetPermissionCatalogAsync()
        {
            var serviceModels = await _permissionService.GetPermissionCatalogAsync();
            var apiModels = _mapper.Map<List<PermissionAPIModel>>(serviceModels);
            return Ok(apiModels);
        }

        [HttpGet("matrix")]
        [ProducesResponseType(typeof(List<RolePermissionMatrixRoleAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetRolePermissionMatrixAsync()
        {
            var serviceModels = await _permissionService.GetRolePermissionMatrixAsync();
            var apiModels = _mapper.Map<List<RolePermissionMatrixRoleAPIModel>>(serviceModels);
            return Ok(apiModels);
        }

        [HttpPut("role")]
        [ProducesResponseType(typeof(OkResult), HttpStatusCodes.OK)]
        public async Task<IActionResult> UpdateRolePermissionsAsync([FromBody] UpdateRolePermissionsAPIModel updateRolePermissionsAPIModel)
        {
            if (updateRolePermissionsAPIModel == null)
            {
                return BadRequest("Request body is required.");
            }

            var serviceModel = _mapper.Map<UpdateRolePermissionsServiceModel>(updateRolePermissionsAPIModel);
            await _permissionService.UpdateRolePermissionsAsync(serviceModel);

            // The future Access Rights admin screen just changed grants - make sure the
            // very next authorized request anywhere sees them immediately rather than
            // waiting out the cache's 10-minute absolute expiration.
            _rolePermissionCache.Invalidate();

            return Ok();
        }
    }
}
