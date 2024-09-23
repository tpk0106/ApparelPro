using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.Reference;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;
using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeService;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/garmentType")]
    [Authorize("RegisteredUser")]
    [ApiController]
    public class GarmentTypeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IGarmentTypeService _garmentTypeService;
        public GarmentTypeController(IMapper mapper, IGarmentTypeService garmentTypeService)
        {
            if (garmentTypeService == null) throw new ArgumentNullException(nameof(garmentTypeService));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            _mapper = mapper;
            _garmentTypeService = garmentTypeService;            
        }

        [HttpGet("list")]
        //  [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [Authorize("Merchandising")] // policy applied
                                     //[Authorize(Roles = "Inventory")]
                                     // [Authorize("RegisteredUser")]
        [ProducesResponseType(typeof(PaginationAPIModel<GarmentTypeAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetGarmentTypesAsync([FromQuery] int pageSize, [FromQuery] 
            int pageNumber, string? sortColumn = null, string? sortOrder = null, 
            string? filterColumn = null, string? filterQuery = null)
        {            
            var garmentTypeServiceModels = await _garmentTypeService.GetGarmentTypesAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var garmentTypes = _mapper.Map<PaginationAPIModel<GarmentTypeAPIModel>>(garmentTypeServiceModels);
            return Ok(garmentTypes);
        }

        [HttpPut()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        //  [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateCountryAsync([FromQuery] int id, [FromBody] UpdateGarmentTypeAPIModel
           updateGarmentTypeAPIModel)
        {
            var resultGarmentTypeAPIModel = _mapper.Map<GarmentTypeAPIModel>(await _garmentTypeService.GetGarmentTypeByIdAsync(id));
            
            if (resultGarmentTypeAPIModel == null)
            {
                return UnprocessableEntity("Country is not available for id :" + id);
            }
            updateGarmentTypeAPIModel.Id = resultGarmentTypeAPIModel.Id;
            var updateGarmentTypeSeviceModel = _mapper.Map<UpdateGarmentTypeServiceModel>(updateGarmentTypeAPIModel);
            await _garmentTypeService.UpdateGarmentTypeAsync(updateGarmentTypeSeviceModel);
            return NoContent();
        }

        [HttpPatch()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateGarmentAsync()
        {
            return NoContent();
        }
    }
}
