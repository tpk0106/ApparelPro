using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Implementation.Shared;
using apparelPro.BusinessLogic.Services.Models.Reference.ISupplierService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using ApparelPro.Shared.LookupConstants.ApparelProContext;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.Extensions;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Implementation.Reference;
using apparelPro.BusinessLogic.Services.Models.Reference.ICountryService;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/supplier")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext; 
        private readonly IDistributedCache _distributedCache;
        private readonly ISupplierService _supplierService;
        public SupplierController(IMapper mapper, ApparelProDbContext apparelProDbContext, 
            IDistributedCache distributedCache, ISupplierService supplierService)
        {
            if (apparelProDbContext == null)
            {
                throw new ArgumentNullException(nameof(apparelProDbContext));
            }
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            if(supplierService == null)
            {
                throw new ArgumentNullException(nameof(supplierService));
            }
            
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
            _supplierService = supplierService;
            _distributedCache = distributedCache;
        }


        [HttpGet("list")]
        //  [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [Authorize("Merchandising")] // policy applied
                                     //[Authorize(Roles = "Inventory")]
                                     // [Authorize("RegisteredUser")]
        [SwaggerOperation(Tags = new[] { "Bank Endpoints" },
            Summary = "list all Bank details with paging and filtering.",
            Description = "Returns 200 - OK with PaginationAPIModel with Bank list.")
        ]
        [ProducesResponseType(typeof(PaginationAPIModel<SupplierAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetSuppliersAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var supplierServiceModels = await _supplierService.GetSuppliersAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var suppliers = _mapper.Map<PaginationAPIModel<SupplierAPIModel>>(supplierServiceModels);
            return Ok(suppliers);
        }

        [HttpGet("list/{SupplierCode}", Name = "GetSupplierBySupplierCodeAsync")]
        [ProducesResponseType(typeof(CountryAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Bank Endpoints" },
           Summary = "list a Bank details for a given Bank Code",
           Description = "Returns 200 - OK with Bank model.")
       ]
        public async Task<IActionResult> GetSupplierBySupplierCodeAsync(int supplierCode)
        {
            var supplier = await _supplierService.GetSupplierBySupplierCodeAsync(supplierCode);
            if (supplier == null)
            {
                return UnprocessableEntity("supplier is not available for code :" + supplierCode);
            }
            var supplierAPIModel = _mapper.Map<SupplierAPIModel>(supplier);
            return Ok(supplierAPIModel);
        }

        [HttpPost]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Supplier Endpoints" },
            Summary = "Add a Suppler.",
            Description = "Returns 200 - OK with No content")
        ]
        public async Task<IActionResult> AddSupplierAsync([FromBody] CreateSupplierAPIModel createSupplierAPIModel)
        {
            var createSupplierServiceModel = _mapper.Map<CreateSupplierServiceModel>(createSupplierAPIModel);
            var addedSupplier = await _supplierService.AddSupplierAsync(createSupplierServiceModel);
            return CreatedAtRoute(nameof(GetSupplierBySupplierCodeAsync), new { SupplierCode = addedSupplier.SupplierCode }, null);
        }

        [HttpDelete("{code}")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Bank Endpoints" },
            Summary = "Delete a Bank.",
            Description = "Returns 200 - OK with No content")
        ]
        public async Task<IActionResult> DeleteSupplierAsync(int supplierCode)
        {
            var supplerServiceModel = await _supplierService.GetSupplierBySupplierCodeAsync(supplierCode);
            if (supplerServiceModel == null)
            {
                return UnprocessableEntity("Supplier is not available for code :" + supplierCode);
            }
            await _supplierService.DeleteSupplierAsync(supplierCode);
            return NoContent();
        }

        [HttpPut()]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Bank Endpoints" },
            Summary = "Update a Bank.",
            Description = "Returns 200 - OK with No content")
        ]
        //  [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateSupplierAsync([FromQuery] int supplierCode, [FromBody] UpdateSupplierAPIModel
            updateSupplierAPIModel)
        {
            var resultSupplierAPIModel = _mapper.Map<SupplierAPIModel>(await _supplierService.GetSupplierBySupplierCodeAsync(supplierCode));
            // Response.Headers.AccessControlAllowOrigin = "*";
            if (resultSupplierAPIModel == null)
            {
                return UnprocessableEntity("Supplier is not available for code :" + supplierCode);
            }         
            var updateSupplierSeviceModel = _mapper.Map<UpdateSupplierServiceModel>(updateSupplierAPIModel);
            await _supplierService.UpdateSupplierAsync(updateSupplierSeviceModel);
            return NoContent();
        }

        [HttpGet("suppliers-lookup")]
        [ProducesResponseType(typeof(List<SupplierLookupAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetSuppliersLookup()
        {
            try
            {
                var data = await _supplierService.GetSuppliersLookupAsync();
                var supplierLookupAPIModels = _mapper.Map<List<SupplierLookupAPIModel>>(data);
                return Ok(supplierLookupAPIModels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to execute supplier repository lookup lookup: {ex.Message}" });
            }
        }

    }
}
