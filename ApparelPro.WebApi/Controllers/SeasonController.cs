using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Reference.ISeasonService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers
{
    // Season master data is seeded from legacy od_sea.dbf (see SeasonConfig) and used by
    // both the Order Confirmation Routine's Season dropdown and the Year/Season Wise
    // Orders report.
    [Route("api/season")]
    [ApiController]
    public class SeasonController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISeasonService _seasonService;

        public SeasonController(IMapper mapper, ISeasonService seasonService)
        {
            _mapper = mapper;
            _seasonService = seasonService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "season-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<SeasonAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Season Endpoints" },
           Summary = "Season list.",
           Description = "Returns 200 - OK with list")
       ]
        public async Task<IActionResult> GetSeasonsAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var seasonServiceModels = await _seasonService.GetSeasonsAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var seasons = _mapper.Map<PaginationAPIModel<SeasonAPIModel>>(seasonServiceModels);
            return Ok(seasons);
        }

        [HttpGet("list/{code}", Name = "GetSeasonByCodeAsync")]
        [Authorize(Policy = "season-view")]
        [ProducesResponseType(typeof(SeasonAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Season Endpoints" },
             Summary = "Season details for a given Season Code",
             Description = "Returns 200 - OK with Season model.")
         ]
        public async Task<IActionResult> GetSeasonByCodeAsync(string code)
        {
            var season = await _seasonService.GetSeasonByCodeAsync(code);
            if (season == null)
            {
                return UnprocessableEntity("Season is not available for code :" + code);
            }
            return Ok(_mapper.Map<SeasonAPIModel>(season));
        }

        [HttpPost()]
        [Authorize(Policy = "season-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Season Endpoints" },
           Summary = "Add a Season.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddSeasonAsync([FromBody] CreateSeasonAPIModel createSeasonAPIModel)
        {
            try
            {
                var existingSeason = await _seasonService.GetSeasonByCodeAsync(createSeasonAPIModel.Code);
                if (existingSeason != null)
                {
                    return BadRequest(new { message = "Season already exists" });
                }
                var createSeasonServiceModel = _mapper.Map<CreateSeasonServiceModel>(createSeasonAPIModel);

                var addedSeason = await _seasonService.AddSeasonAsync(createSeasonServiceModel);
                var seasonAPIModel = _mapper.Map<CreateSeasonAPIModel>(addedSeason);
                return CreatedAtRoute(nameof(GetSeasonByCodeAsync), new { code = seasonAPIModel.Code }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpPut()]
        [Authorize(Policy = "season-manage")]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Season Endpoints" },
           Summary = "Update a Season.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> UpdateSeasonAsync([FromQuery] string code, [FromBody] UpdateSeasonAPIModel updateSeasonAPIModel)
        {
            try
            {
                var existingSeason = await _seasonService.GetSeasonByCodeAsync(code);
                if (existingSeason == null)
                {
                    return UnprocessableEntity("Season is not available for code :" + code);
                }
                updateSeasonAPIModel.Code = existingSeason.Code;
                var updateSeasonServiceModel = _mapper.Map<UpdateSeasonServiceModel>(updateSeasonAPIModel);
                await _seasonService.UpdateSeasonAsync(updateSeasonServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "season-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Season Endpoints" },
           Summary = "Delete a Season.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeleteSeasonAsync(string code)
        {
            var existingSeason = await _seasonService.GetSeasonByCodeAsync(code);
            if (existingSeason == null)
            {
                return UnprocessableEntity("Season is not available for code :" + code);
            }
            try
            {
                await _seasonService.DeleteSeasonAsync(code);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Most likely FK_PurchaseOrders_Seasons_Season - a Season still referenced
                // by a PurchaseOrder can't be deleted.
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
