using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using apparelPro.BusinessLogic.Services.Models.Reference.IBankService;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/bank")]
    [ApiController]
   // [Authorize("RegisteredUser")]
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
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
    //    [Authorize("Merchandising")] // policy applied
        //[Authorize(Roles = "Inventory")]
       //  [Authorize("RegisteredUser")]
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
            var banks = _mapper.Map<PaginationAPIModel<BankAPIModel>>(countryServiceModels);
            return Ok(banks);
        }


        [HttpPost()]
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Bank Endpoints" },
           Summary = "Add a Bank.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddBankAsync([FromBody] CreateBankAPIModel createBankAPIModel)
        {
            var createBankServiceModel = _mapper.Map<CreateBankServiceModel>(createBankAPIModel);
            var addedBank = await _bankService.AddBankAsync(createBankServiceModel);
            return CreatedAtRoute(nameof(GetBankByBankCodeAsync), new { code = addedBank.BankCode }, null);
        }

        [HttpGet("list/{code}", Name = "GetBankByBankCodeAsync")]
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(typeof(BankAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Bank Endpoints" },
             Summary = "list a Bank details for a given Bank Code",
             Description = "Returns 200 - OK with Bank model.")
         ]
        public async Task<IActionResult> GetBankByBankCodeAsync(string code)
        {
            var Bank = await _bankService.GetBankByBankCodeAsync(code);
            if (Bank == null)
            {
                return UnprocessableEntity("Bank is not available for code :" + code);
            }
            var bankAPIModel = _mapper.Map<BankAPIModel>(Bank);
            return Ok(bankAPIModel);
        }

        [HttpPut()]
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Bank Endpoints" },
            Summary = "Update a Bank.",
            Description = "Returns 200 - OK with No content")
        ]        
        public async Task<IActionResult> UpdateBankAsync([FromQuery] string bankCode, [FromBody] UpdateBankAPIModel
            updateBankAPIModel)
        {
            var resultBankAPIModel = _mapper.Map<BankAPIModel>(await _bankService.GetBankByBankCodeAsync(bankCode));
            // Response.Headers.AccessControlAllowOrigin = "*";
            if (resultBankAPIModel == null)
            {
                return UnprocessableEntity("Country is not available for code :" + bankCode);
            }
            updateBankAPIModel.BankCode = resultBankAPIModel.BankCode;
            var updateBankSeviceModel = _mapper.Map<UpdateBankServiceModel>(updateBankAPIModel);
            await _bankService.UpdateBankAsync(updateBankSeviceModel);
            return NoContent();
        }
    }
}
