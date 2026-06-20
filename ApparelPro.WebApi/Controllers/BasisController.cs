using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Reference.IBasisService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
        [ProducesResponseType(HttpStatusCodes.OK)]        
        [Authorize("Merchandising")] // policy applied                                  
        [ProducesResponseType(typeof(PaginationAPIModel<BasisAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Basis Endpoints" },
           Summary = "Basis list.",
           Description = "Returns 200 - OK with list")
       ]
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

        [HttpPost()]
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Basis Endpoints" },
           Summary = "Add a Basis.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddBankAsync([FromBody] CreateBasisAPIModel createBasisAPIModel)
        {
            try
            {
                var basis = _basisService.GetBasisByCodeAsync(createBasisAPIModel.Code);
                if (basis != null)
                {
                    return BadRequest(new { message = "Basis already exists" });
                }
                var createBasisServiceModel = _mapper.Map<CreateBasisServiceModel>(createBasisAPIModel);

                var addedBasis = await _basisService.AddBasisAsync(createBasisServiceModel);
                var basisAPIModel = _mapper.Map<CreateBasisAPIModel>(addedBasis);
                return CreatedAtRoute(nameof(GetBasisByCodeAsync), new { code = basisAPIModel.Code }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("list/{code}", Name = "GetBasisByCodeAsync")]
        [Authorize(Roles = "Inventory, Merchandiser,Merchandiser Manager,Order Entry Operator")]
        [ProducesResponseType(typeof(BasisAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Basis Endpoints" },
             Summary = "Basis details for a given Basis Code",
             Description = "Returns 200 - OK with Basis model.")
         ]
        public async Task<IActionResult> GetBasisByCodeAsync(string code)
        {
            try
            {
                var basisAPIModel = _mapper.Map<BasisAPIModel>(await _basisService.GetBasisByCodeAsync(code));
                if (basisAPIModel == null)
                {
                    return UnprocessableEntity("Basis is not available for code :" + code);
                }
                //var basisAPIModel = _mapper.Map<BasisAPIModel>(basis);
                return Ok(basisAPIModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
                //return UnprocessableEntity("Basis is not available for code :" + code);
            }
        }

        [HttpDelete()]        
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Basis Endpoints" },
           Summary = "Delete a Basis.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeleteBasisAsync([FromQuery] string code)
        {
            var basis = await _basisService.GetBasisByCodeAsync(code);
            if (basis == null)
            {
                return UnprocessableEntity("Basis is not available for code :" + code);
            }
            await _basisService.DeleteBasisAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Basis Endpoints" },
           Summary = "Update a Basis.",
           Description = "Returns 200 - OK with No content")
       ]        
        public async Task<IActionResult> UpdateBasisAsync([FromQuery] string code, [FromBody] UpdateBasisAPIModel
           updateBasisAPIModel)
        {
            try
            {
                var resultBasisAPIModel = _mapper.Map<BasisAPIModel>(await _basisService.GetBasisByCodeAsync(code));
                if (resultBasisAPIModel == null)
                {
                    return UnprocessableEntity("basis is not available for code :" + code);
                }
                updateBasisAPIModel.Code = resultBasisAPIModel.Code;
                var updateBasisSeviceModel = _mapper.Map<UpdateBasisServiceModel>(updateBasisAPIModel);
                await _basisService.UpdateBasisAsync(updateBasisSeviceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }          
        }
    }
}
