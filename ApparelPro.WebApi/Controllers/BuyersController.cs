using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Reference.IBuyerService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/buyer")]
    [ApiController]
    public class BuyersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IBuyerService _buyerService;
        public BuyersController(IMapper mapper, IBuyerService buyerService) {
            _mapper = mapper;
            _buyerService = buyerService;
        }       

        [HttpGet("list")]
        //  [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [Authorize("Merchandising")] // policy applied
        
        // [Authorize("RegisteredUser")]
        [ProducesResponseType(typeof(PaginationAPIModel<BuyerAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetBuyersAsync(
           [FromQuery] int pageSize,
           [FromQuery] int pageNumber,
           [FromQuery] string? sortColumn = null,
           [FromQuery] string? sortOrder = null,
           [FromQuery] string? filterColumn = null,
           [FromQuery] string? filterQuery = null)
        {
            var buyerServiceModels = await _buyerService.GetBuyersAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var buyers = _mapper.Map<PaginationAPIModel<BuyerAPIModel>>(buyerServiceModels);
            return Ok(buyers);
        }

        [HttpGet("list-1")]
        [ProducesResponseType(typeof(IEnumerable<BuyerAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetBuyersAsync()
        {
            var buyerServiceModels = await _buyerService.GetBuyersExAsync();
            var buyers = _mapper.Map<IEnumerable<BuyerAPIModel>>(buyerServiceModels);
            return Ok(buyers);

        }

       [HttpPost]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddBuyerAsync([FromBody] CreateBuyerAPIModel createBuyerAPIModel)
        {
            var createBuyerServiceModel = _mapper.Map<CreateBuyerServiceModel>(createBuyerAPIModel);
            var addedBuyer = await _buyerService.AddBuyerAsync(createBuyerServiceModel);
            return CreatedAtRoute(nameof(GetBuyerByBuyerCodeAsync), new { buyerCode = addedBuyer.BuyerCode }, null);
        }

        [HttpGet("list/{buyerCode}", Name = "GetBuyerByBuyerCodeAsync")]
        [ProducesResponseType(typeof(BuyerAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetBuyerByBuyerCodeAsync(int buyerCode)
        {
            var buyer = await _buyerService.GetBuyerByBuyerCodeAsync(buyerCode);
            if (buyer == null)
            {
                return UnprocessableEntity("buyer is not available for buyer code :" + buyerCode);
            }
            var buyerAPIModel = _mapper.Map<BuyerAPIModel>(buyer);
            return Ok(buyerAPIModel);
        }
    }
}
