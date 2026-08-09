using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.ISubContractorService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/sub-contractor")]
    [ApiController]
    public class SubContractorController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISubContractorService _subContractorService;

        public SubContractorController(IMapper mapper, ISubContractorService subContractorService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _subContractorService = subContractorService ?? throw new ArgumentNullException(nameof(subContractorService));
        }

        [HttpGet("list")]
        [Authorize(Policy = "sub-contractor-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<SubContractorAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetSubContractorsAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var subContractorServiceModels = await _subContractorService.GetSubContractorsAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var subContractors = _mapper.Map<PaginationAPIModel<SubContractorAPIModel>>(subContractorServiceModels);
            return Ok(subContractors);
        }

        [HttpGet("list/{code}", Name = "GetSubContractorByCodeAsync")]
        [Authorize(Policy = "sub-contractor-view")]
        [ProducesResponseType(typeof(SubContractorAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetSubContractorByCodeAsync(string code)
        {
            var subContractor = await _subContractorService.GetSubContractorByCodeAsync(code);
            if (subContractor == null)
            {
                return UnprocessableEntity("Sub Contractor is not available for code : " + code);
            }
            var subContractorAPIModel = _mapper.Map<SubContractorAPIModel>(subContractor);
            return Ok(subContractorAPIModel);
        }

        [HttpGet("list/does-exist/{code}", Name = "DoesSubContractorExistAsync")]
        [Authorize(Policy = "sub-contractor-view")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> DoesSubContractorExistAsync(string code)
        {
            var exists = await _subContractorService.DoesSubContractorExistAsync(code);
            return Ok(exists);
        }

        [HttpPost]
        [Authorize(Policy = "sub-contractor-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> AddSubContractorAsync([FromBody] CreateSubContractorAPIModel createSubContractorAPIModel)
        {
            try
            {
                var createSubContractorServiceModel = _mapper.Map<CreateSubContractorServiceModel>(createSubContractorAPIModel);
                var addedSubContractor = await _subContractorService.AddSubContractorAsync(createSubContractorServiceModel);
                return CreatedAtRoute(nameof(GetSubContractorByCodeAsync), new { code = addedSubContractor.Code }, null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "sub-contractor-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteSubContractorAsync([FromQuery] string code)
        {
            var subContractor = await _subContractorService.GetSubContractorByCodeAsync(code);
            if (subContractor == null)
            {
                return UnprocessableEntity("Sub Contractor is not available for code : " + code);
            }
            await _subContractorService.DeleteSubContractorAsync(code);
            return NoContent();
        }

        [HttpPut]
        [Authorize(Policy = "sub-contractor-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateSubContractorAsync([FromQuery] string code, [FromBody] UpdateSubContractorAPIModel updateSubContractorAPIModel)
        {
            var existing = await _subContractorService.GetSubContractorByCodeAsync(code);
            if (existing == null)
            {
                return UnprocessableEntity("Sub Contractor is not available for code : " + code);
            }
            updateSubContractorAPIModel.Code = code;
            var updateSubContractorServiceModel = _mapper.Map<UpdateSubContractorServiceModel>(updateSubContractorAPIModel);
            await _subContractorService.UpdateSubContractorAsync(updateSubContractorServiceModel);
            return NoContent();
        }
    }
}
