using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IPackingListService;
using ApparelPro.WebApi.APIModels.ImportExport;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/packing-list")]
    [ApiController]
    public class PackingListController : ControllerBase
    {
        private readonly IPackingListService _packingListService;
        private readonly IMapper _mapper;

        public PackingListController(IPackingListService packingListService, IMapper mapper)
        {
            _packingListService = packingListService;
            _mapper = mapper;
        }

        // GET: api/packing-list/detail?invoiceNumber=&buyerCode=&order=&typeCode=&styleCode=&newOrder=
        [HttpGet("detail")]
        [Authorize(Policy = "packing-list-view")]
        public async Task<IActionResult> GetByLineKeyAsync(
            [FromQuery] string invoiceNumber, [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode, [FromQuery] string newOrder)
        {
            var detail = await _packingListService.GetByLineKeyAsync(invoiceNumber, buyerCode, order, typeCode, styleCode, newOrder);
            return Ok(_mapper.Map<PackingListDetailAPIModel>(detail));
        }

        [HttpPut]
        [Authorize(Policy = "packing-list-manage")]
        public async Task<IActionResult> SaveAsync([FromBody] SavePackingListAPIModel apiModel)
        {
            var serviceModel = _mapper.Map<SavePackingListServiceModel>(apiModel);
            var saved = await _packingListService.SaveAsync(serviceModel);
            return Ok(_mapper.Map<PackingListDetailAPIModel>(saved));
        }
    }
}
