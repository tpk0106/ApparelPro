using apparelPro.BusinessLogic.Services.interfaces.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.IStockService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers
{
    // Reference Files > C. Inventory Control > A. Stock Reference (OD_STK1.PRG / OD_STK2.PRG) -
    // the master Stock category list (01 RAW MATERIAL, 02 ACCESSORIES, etc.) that the
    // Order Items catalog and Garment Type Item Requirements pickers key off of.
    [Route("api/stock")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IStockService _stockService;

        public StockController(IMapper mapper, IStockService stockService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
        }

        [HttpGet("list")]
        [Authorize(Policy = "stock-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<StockAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetStocksAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var stockServiceModels = await _stockService.GetStocksAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var stocks = _mapper.Map<PaginationAPIModel<StockAPIModel>>(stockServiceModels);
            return Ok(stocks);
        }

        [HttpGet("list/{stockCode}", Name = "GetStockByCodeAsync")]
        [Authorize(Policy = "stock-view")]
        [ProducesResponseType(typeof(StockAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetStockByCodeAsync(string stockCode)
        {
            var stock = await _stockService.GetStockByCodeAsync(stockCode);
            if (stock == null)
            {
                return UnprocessableEntity("Stock Reference is not available for code : " + stockCode);
            }
            var stockAPIModel = _mapper.Map<StockAPIModel>(stock);
            return Ok(stockAPIModel);
        }

        [HttpGet("list/does-exist/{stockCode}", Name = "DoesStockExistAsync")]
        [Authorize(Policy = "stock-view")]
        [ProducesResponseType(typeof(bool), HttpStatusCodes.OK)]
        public async Task<IActionResult> DoesStockExistAsync(string stockCode)
        {
            var exists = await _stockService.DoesStockExistAsync(stockCode);
            return Ok(exists);
        }

        [HttpPost]
        [Authorize(Policy = "stock-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [ProducesResponseType(HttpStatusCodes.BadRequest)]
        public async Task<IActionResult> AddStockAsync([FromBody] CreateStockAPIModel createStockAPIModel)
        {
            try
            {
                var createStockServiceModel = _mapper.Map<CreateStockServiceModel>(createStockAPIModel);
                var addedStock = await _stockService.AddStockAsync(createStockServiceModel);
                return CreatedAtRoute(nameof(GetStockByCodeAsync), new { stockCode = addedStock.StockCode }, null);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [Authorize(Policy = "stock-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteStockAsync([FromQuery] string stockCode)
        {
            var stock = await _stockService.GetStockByCodeAsync(stockCode);
            if (stock == null)
            {
                return UnprocessableEntity("Stock Reference is not available for code : " + stockCode);
            }
            try
            {
                await _stockService.DeleteStockAsync(stockCode);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            return NoContent();
        }

        [HttpPut]
        [Authorize(Policy = "stock-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateStockAsync([FromQuery] string stockCode, [FromBody] UpdateStockAPIModel updateStockAPIModel)
        {
            var existing = await _stockService.GetStockByCodeAsync(stockCode);
            if (existing == null)
            {
                return UnprocessableEntity("Stock Reference is not available for code : " + stockCode);
            }
            updateStockAPIModel.StockCode = stockCode;
            var updateStockServiceModel = _mapper.Map<UpdateStockServiceModel>(updateStockAPIModel);
            await _stockService.UpdateStockAsync(updateStockServiceModel);
            return NoContent();
        }
    }
}
