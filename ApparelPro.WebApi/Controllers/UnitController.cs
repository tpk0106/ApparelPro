using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Reference.IUnitService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/unit")]
    [ApiController]
    [Authorize("RegisteredUser")]
    public class UnitController : ControllerBase
    {
        //private readonly IUnitServiceT<UnitServiceModel> _unitService;
        private readonly IUnitService _unitService;
      //  private readonly IUnitService _unitService;
        private readonly IMapper _mapper;

        public UnitController(IUnitService unitService,/*IUnitServiceT<UnitServiceModel> unitService*/  IMapper mapper)
        {
            _unitService = unitService;
            _mapper = mapper;            
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(PaginationAPIModel<UnitAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetUnitsAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var unitServiceModels = await _unitService.GetUnitsAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var units = _mapper.Map<PaginationAPIModel<UnitAPIModel>>(unitServiceModels);
            return Ok(units);          
        }

        [HttpPost]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddUnitAsync([FromBody] CreateUnitAPIModel createUnitAPIModel)
        {
            var createUnitServiceModel = _mapper.Map<CreateUnitServiceModel>(createUnitAPIModel);
            var addedUnit = await _unitService.AddUnitAsync(createUnitServiceModel);
            return CreatedAtRoute(nameof(GetUnitByCodeAsync), new { code = addedUnit.Code }, null);
        }

        [HttpPut()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        //  [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateUnitAsync([FromQuery] string code, [FromBody] UpdateUnitAPIModel
         updateUnitAPIModel)
        {
            var resultUnitAPIModel = _mapper.Map<UnitAPIModel>(await _unitService.GetUnitByCodeAsync(code));
            
            if (resultUnitAPIModel == null)
            {
                return UnprocessableEntity("Unit is not available for code :" + code);
            }
            updateUnitAPIModel.Id = resultUnitAPIModel.Id;
            var updateUnitSeviceModel = _mapper.Map<UpdateUnitServiceModel>(updateUnitAPIModel);
            await _unitService.UpdateUnitAsync(updateUnitSeviceModel);
            return NoContent();
        }

        [HttpGet("list/{code}", Name = "GetUnitByCodeAsync")]
        [ProducesResponseType(typeof(UnitAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetUnitByCodeAsync(string code)
        {
            var unitServiceModel = await _unitService.GetUnitByCodeAsync(code);
            var unitAPIModels = _mapper.Map<IEnumerable<UnitAPIModel>>(unitServiceModel);
            return Ok(unitServiceModel);
        }

        [HttpGet("list/does-unit-exist/{code}", Name = "DoesUnitExistAsync")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> DoesUnitExistAsync(string code)
        {
            var exist = await _unitService.DoesUnitExistAsync(code);
            return Ok(exist);
        }
    }
}
