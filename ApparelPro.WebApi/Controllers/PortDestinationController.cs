using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApparelPro.WebApi.Authorization;
using apparelPro.BusinessLogic.Services.Models.Reference.IPortDestinationService;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/portDestination")]
    [ApiController]
    public class PortDestinationController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IPortDestinationService _portDestinationService;
        public PortDestinationController(IMapper mapper, IPortDestinationService portDestinationService)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            if (portDestinationService == null)
            {
                throw new ArgumentNullException(nameof(portDestinationService));
            }
            _mapper = mapper;
            _portDestinationService = portDestinationService;
        }

        [HttpGet("list")]
        [Authorize("Merchandising")] // policy applied
        // [Authorize("RegisteredUser")]
        [ProducesResponseType(typeof(PaginationAPIModel<PortDestinationAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetPortDestinationsAsync(
           [FromQuery] int pageSize,
           [FromQuery] int pageNumber,
           [FromQuery] string? sortColumn = null,
           [FromQuery] string? sortOrder = null,
           [FromQuery] string? filterColumn = null,
           [FromQuery] string? filterQuery = null)
        {
            var portDestinationServiceModels = await _portDestinationService.GetPortDestinationsAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var portDestinations = _mapper.Map<PaginationAPIModel<PortDestinationAPIModel>>(portDestinationServiceModels);
            return Ok(portDestinations);
        }

        [HttpPost]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddPortDestinationAsync([FromBody] CreatePortDestinationAPIModel createPortDestinationAPIModel)
        {
            var createPortDestinationServiceModel = _mapper.Map<CreatePortDestinationServiceModel>(createPortDestinationAPIModel);
            var addedPortDestination = await _portDestinationService.AddPortDestinationAsync(createPortDestinationServiceModel);
            return CreatedAtRoute(nameof(GetPortDestinationByIdAndCountryCodeAsync),
                new { id = addedPortDestination.Id, countryCode = addedPortDestination.CountryCode }, null);
        }

        [HttpGet("list/{id}/{countryCode}", Name = "GetPortDestinationByIdAndCountryCodeAsync")]
        [ProducesResponseType(typeof(PortDestinationAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetPortDestinationByIdAndCountryCodeAsync(int id, string countryCode)
        {
            var portDestination = await _portDestinationService.GetPortDestinationByIdAndCountryCodeAsync(id, countryCode);
            if (portDestination == null)
            {
                return UnprocessableEntity("port destination is not available for country code :" + countryCode);
            }
            var portDestinationAPIModel = _mapper.Map<PortDestinationAPIModel>(portDestination);
            return Ok(portDestinationAPIModel);
        }

        [HttpPut()]
        [Authorize(Roles = AccessPolicies.OrderwiseInventoryStandard)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]        
        public async Task<IActionResult> UpdateBuyerAsync([FromQuery] int id,string countryCode, [FromBody] UpdatePortDestinationAPIModel
           updatePortDestinationAPIModel)
        {
            var resultPortDestinationAPIModel = _mapper.Map<PortDestinationAPIModel>(await _portDestinationService.GetPortDestinationByIdAndCountryCodeAsync(id, countryCode));

            if (resultPortDestinationAPIModel == null)
            {
                return UnprocessableEntity("Port Destination is not available for country :" + countryCode);
            }
            updatePortDestinationAPIModel.CountryCode = resultPortDestinationAPIModel.CountryCode;
            var updatePortDestinationSeviceModel = _mapper.Map<UpdatePortDestinationServiceModel>(updatePortDestinationAPIModel);
            await _portDestinationService.UpdatePortDestinationAsync(updatePortDestinationSeviceModel);
            return NoContent();
        }

        [HttpDelete("{id}/{countryCode}")]
        [Authorize(Roles = AccessPolicies.OrderwiseInventoryStandard)]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeletePortDestinationAsync(int id, string countryCode )
        {
            var portDestination = await _portDestinationService.GetPortDestinationByIdAndCountryCodeAsync(id, countryCode);
            if (portDestination == null)
            {
                return UnprocessableEntity("Port Destination is not available for code :" + countryCode);
            }
            await _portDestinationService.DeletePortDestinationAsync(id, countryCode);
            return NoContent();
        }
    }
}
