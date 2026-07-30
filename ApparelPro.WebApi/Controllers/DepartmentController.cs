using apparelPro.BusinessLogic.Services;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Reference;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace ApparelPro.WebApi.Controllers
{
    [Route("api/department")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly IMapper _mapper;
        public DepartmentController(IDepartmentService departmentService, IMapper mapper)
        {
            _departmentService = departmentService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "department-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<DepartmentAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetDepartmentsAsync(
          [FromQuery] int pageSize,
          [FromQuery] int pageNumber,
          [FromQuery] string? sortColumn = null,
          [FromQuery] string? sortOrder = null,
          [FromQuery] string? filterColumn = null,
          [FromQuery] string? filterQuery = null)
        {
            var departmentServiceModels = await _departmentService.GetDepartmentsAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var departments = _mapper.Map<PaginationAPIModel<DepartmentAPIModel>>(departmentServiceModels);
            return Ok(departments);
        }

        // GET: api/department/lookup
        [HttpGet("lookup")]
        [Authorize(Policy = "department-view")]
        [ProducesResponseType(typeof(IEnumerable<DepartmentAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetDepartmentsLookup()
        {
            try
            {
                // Queries your seeded od_dept table records directly out of SQL Server!
                var departments = await _departmentService.GetDepartmentsLookup();
                var departmentAPIModels = _mapper.Map<IEnumerable<DepartmentAPIModel>>(departments);

                return Ok(departmentAPIModels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to retrieve master department reference listings: {ex.Message}" });
            }
        }
    }
}
