using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Reference.IGarmentTypeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/garmentType")]
    //[Authorize("RegisteredUser")]
    [Authorize(Roles = "Merchandiser, Merchandiser Manager")]
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
          [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        //[Authorize("Merchandiser")] // policy applied
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

        [HttpGet("list/all")]
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]        
        [ProducesResponseType(typeof(IEnumerable<GarmentTypeAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetAllGarmentTypeAsync()
        {
            var garmentTypeServiceModels = await _garmentTypeService.GetAllGarmentTypeAsync();
            var garmentTypes = _mapper.Map<IEnumerable<GarmentTypeAPIModel>>(garmentTypeServiceModels);
            return Ok(garmentTypes);
        }

        [HttpGet("list/{id}", Name = "GetGarmentTypeByIdAsync")]
        [ProducesResponseType(typeof(GarmentTypeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetGarmentTypeByIdAsync(int id)
        {
            //Response.Headers.AccessControlAllowOrigin = "*";
            var garmentType = await _garmentTypeService.GetGarmentTypeByIdAsync(id);
            if (garmentType == null)
            {
                return UnprocessableEntity("garmentType is not available for id :" + id);
            }
            var garmentTypeAPIModel = _mapper.Map<GarmentTypeAPIModel>(garmentType);
            return Ok(garmentTypeAPIModel);
        }

        [HttpPut()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        
        public async Task<IActionResult> UpdateGarmentTypeAsync([FromQuery] int id, [FromBody] UpdateGarmentTypeAPIModel
           updateGarmentTypeAPIModel)
        {
            var resultGarmentTypeAPIModel = _mapper.Map<GarmentTypeAPIModel>(await _garmentTypeService.GetGarmentTypeByIdAsync(id));
            
            if (resultGarmentTypeAPIModel == null)
            {
                return UnprocessableEntity("GarmentType is not available for id :" + id);
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

        [HttpPost]
        [ProducesResponseType(typeof(CreatedResult),  HttpStatusCodes.Created)]
        public async Task<IActionResult> AddGarmentTypeAsync([FromBody] CreateGarmentTypeAPIModel createGarmentTypeAPIModel)
        {
            try
            {
                var createGarmentTypeServiceModel = _mapper.Map<CreateGarmentTypeServiceModel>(createGarmentTypeAPIModel);
                var adddedGarmentType = await _garmentTypeService.CreateGarmentTypeAsync(createGarmentTypeServiceModel);

                return CreatedAtRoute(nameof(GetGarmentTypeByIdAsync), new { adddedGarmentType.Id }, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
