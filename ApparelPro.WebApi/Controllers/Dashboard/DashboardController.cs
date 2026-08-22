using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels.Dashboard;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Dashboard
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IMapper _mapper;

        public DashboardController(IDashboardService dashboardService, IMapper mapper)
        {
            _dashboardService = dashboardService;
            _mapper = mapper;
        }

        [HttpGet("current-style")]
        [Authorize(Policy = "dashboard-view")]
        [ProducesResponseType(typeof(CurrentStyleAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        public async Task<IActionResult> GetCurrentStyleAsync()
        {
            var serviceModel = await _dashboardService.GetCurrentStyleAsync();
            if (serviceModel == null) return NoContent();
            return Ok(_mapper.Map<CurrentStyleAPIModel>(serviceModel));
        }

        [HttpGet("production-progress")]
        [Authorize(Policy = "dashboard-view")]
        [ProducesResponseType(typeof(ProductionProgressAPIModel), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetProductionProgressAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            var serviceModel = await _dashboardService.GetProductionProgressAsync(
                buyerCode, order, typeCode, styleCode);
            return Ok(_mapper.Map<ProductionProgressAPIModel>(serviceModel));
        }

        [HttpGet("daily-trend-all-sections")]
        [Authorize(Policy = "dashboard-view")]
        [ProducesResponseType(typeof(List<DailyTrendSeriesAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetDailyTrendAllSectionsAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode, [FromQuery] int days = 8)
        {
            var serviceModels = await _dashboardService.GetDailyTrendAllSectionsAsync(
                buyerCode, order, typeCode, styleCode, days);
            return Ok(_mapper.Map<List<DailyTrendSeriesAPIModel>>(serviceModels));
        }

        [HttpGet("order-management-summary")]
        [Authorize(Policy = "dashboard-view")]
        [ProducesResponseType(typeof(OrderManagementSummaryAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetOrderManagementSummaryAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode)
        {
            var serviceModel = await _dashboardService.GetOrderManagementSummaryAsync(
                buyerCode, order, typeCode, styleCode);
            if (serviceModel == null) return NotFound();
            return Ok(_mapper.Map<OrderManagementSummaryAPIModel>(serviceModel));
        }

        [HttpGet("orderwise-inventory-summary")]
        [Authorize(Policy = "dashboard-view")]
        [ProducesResponseType(typeof(OrderwiseInventorySummaryAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(HttpStatusCodes.NotFound)]
        public async Task<IActionResult> GetOrderwiseInventorySummaryAsync(
            [FromQuery] int buyerCode, [FromQuery] string order)
        {
            var serviceModel = await _dashboardService.GetOrderwiseInventorySummaryAsync(buyerCode, order);
            if (serviceModel == null) return NotFound();
            return Ok(_mapper.Map<OrderwiseInventorySummaryAPIModel>(serviceModel));
        }

        [HttpGet("daily-trend")]
        [Authorize(Policy = "dashboard-view")]
        [ProducesResponseType(typeof(List<DailyTrendPointAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetDailyTrendAsync(
            [FromQuery] int buyerCode, [FromQuery] string order,
            [FromQuery] int typeCode, [FromQuery] string styleCode,
            [FromQuery] string? sectionCode = null, [FromQuery] int days = 8)
        {
            var serviceModels = await _dashboardService.GetDailyTrendAsync(
                buyerCode, order, typeCode, styleCode, sectionCode, days);
            return Ok(_mapper.Map<List<DailyTrendPointAPIModel>>(serviceModels));
        }
    }
}
