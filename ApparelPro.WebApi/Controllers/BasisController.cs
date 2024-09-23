using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/basis")]
    [ApiController]
    [Authorize("RegisteredUser")]
    public class BasisController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IBasisService _basisService;
        public BasisController(IMapper mapper, IBasisService basisService)
        {
            _mapper = mapper;
            _basisService = basisService;
        }

        [HttpGet("list")]
        //[Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [Authorize("Merchandising")] // policy applied                                  
        [ProducesResponseType(typeof(PaginationAPIModel<BasisAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetBasisesAsync(
            [FromQuery] int pageSize, 
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
        )
        {
            var basisServiceModels = await _basisService.GetBasisesAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var basises = _mapper.Map<PaginationAPIModel<BasisAPIModel>>(basisServiceModels);
            return Ok(basises);
        }
    }
}
