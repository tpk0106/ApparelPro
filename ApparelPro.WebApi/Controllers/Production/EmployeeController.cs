using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.Production.IEmployeeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.Production;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApparelPro.WebApi.Controllers.Production
{
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;

        public EmployeeController(IEmployeeService employeeService, IMapper mapper)
        {
            _employeeService = employeeService;
            _mapper = mapper;
        }

        [HttpGet("list")]
        [Authorize(Policy = "employee-view")]
        [ProducesResponseType(typeof(PaginationAPIModel<EmployeeAPIModel>), HttpStatusCodes.OK)]
        public async Task<IActionResult> GetEmployeesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null)
        {
            var serviceModels = await _employeeService.GetEmployeesAsync(pageNumber, pageSize,
                sortColumn, sortOrder, filterColumn, filterQuery);
            var itemsPage = _mapper.Map<PaginationAPIModel<EmployeeAPIModel>>(serviceModels);
            return Ok(itemsPage);
        }

        [HttpGet("list/{employeeCode}", Name = "GetEmployeeByEmployeeCodeAsync")]
        [Authorize(Policy = "employee-view")]
        [ProducesResponseType(typeof(EmployeeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> GetEmployeeByEmployeeCodeAsync(string employeeCode)
        {
            var employee = await _employeeService.GetEmployeeByEmployeeCodeAsync(employeeCode);
            if (employee == null)
            {
                return UnprocessableEntity("Employee is not available for code :" + employeeCode);
            }
            return Ok(_mapper.Map<EmployeeAPIModel>(employee));
        }

        [HttpPost]
        [Authorize(Policy = "employee-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        public async Task<IActionResult> AddEmployeeAsync([FromBody] CreateEmployeeAPIModel createAPIModel)
        {
            var createServiceModel = _mapper.Map<CreateEmployeeServiceModel>(createAPIModel);
            var added = await _employeeService.AddEmployeeAsync(createServiceModel);
            return CreatedAtRoute(nameof(GetEmployeeByEmployeeCodeAsync), new { employeeCode = added.EmployeeCode }, null);
        }

        [HttpPut]
        [Authorize(Policy = "employee-manage")]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        public async Task<IActionResult> UpdateEmployeeAsync([FromQuery] string employeeCode, [FromBody] UpdateEmployeeAPIModel updateAPIModel)
        {
            var existing = await _employeeService.GetEmployeeByEmployeeCodeAsync(employeeCode);
            if (existing == null)
            {
                return UnprocessableEntity("Employee is not available for code :" + employeeCode);
            }
            updateAPIModel.EmployeeCode = employeeCode;
            var updateServiceModel = _mapper.Map<UpdateEmployeeServiceModel>(updateAPIModel);
            await _employeeService.UpdateEmployeeAsync(updateServiceModel);
            return NoContent();
        }

        [HttpDelete("{employeeCode}")]
        [Authorize(Policy = "employee-manage")]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        public async Task<IActionResult> DeleteEmployeeAsync(string employeeCode)
        {
            var existing = await _employeeService.GetEmployeeByEmployeeCodeAsync(employeeCode);
            if (existing == null)
            {
                return UnprocessableEntity("Employee is not available for code :" + employeeCode);
            }
            await _employeeService.DeleteEmployeeAsync(employeeCode);
            return NoContent();
        }
    }
}
