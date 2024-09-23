using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.Reference;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/bank")]
    [ApiController]
    [Authorize("RegisteredUser")]
    public class BankController : ControllerBase
    {
        private readonly IBankService _bankService;
        private readonly IMapper _mapper;
        public BankController(IBankService bankService, IMapper mapper)
        {
            _bankService = bankService;
            _mapper = mapper;            
        }

        [HttpGet("list")]
        //  [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [Authorize("Merchandising")] // policy applied
        //[Authorize(Roles = "Inventory")]
        // [Authorize("RegisteredUser")]
        [ProducesResponseType(typeof(PaginationAPIModel<BankAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetCountriesAsync(
          [FromQuery] int pageSize,
          [FromQuery] int pageNumber,
          [FromQuery] string? sortColumn = null,
          [FromQuery] string? sortOrder = null,
          [FromQuery] string? filterColumn = null,
          [FromQuery] string? filterQuery = null)
        {
            var countryServiceModels = await _bankService.GetBanksAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var countries = _mapper.Map<PaginationAPIModel<BankAPIModel>>(countryServiceModels);
            return Ok(countries);
        }
    }
}
