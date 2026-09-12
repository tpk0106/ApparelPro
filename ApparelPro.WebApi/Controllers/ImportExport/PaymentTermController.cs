using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IPaymentTermService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/payment-term")]
    [ApiController]
    public class PaymentTermController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IPaymentTermService _paymentTermService;
        public PaymentTermController(IMapper mapper, IPaymentTermService paymentTermService)
        {
            _mapper = mapper;
            _paymentTermService = paymentTermService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "payment-term-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(PaginationAPIModel<PaymentTermAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Payment Term Endpoints" },
           Summary = "Payment Term list.",
           Description = "Returns 200 - OK with list")
       ]
        public async Task<IActionResult> GetPaymentTermsAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
        )
        {
            var paymentTermServiceModels = await _paymentTermService.GetPaymentTermsAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var paymentTerms = _mapper.Map<PaginationAPIModel<PaymentTermAPIModel>>(paymentTermServiceModels);
            return Ok(paymentTerms);
        }

        [HttpPost()]
        [Authorize(Policy = "payment-term-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Payment Term Endpoints" },
           Summary = "Add a Payment Term.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddPaymentTermAsync([FromBody] CreatePaymentTermAPIModel createPaymentTermAPIModel)
        {
            try
            {
                var paymentTerm = await _paymentTermService.GetPaymentTermByCodeAsync(createPaymentTermAPIModel.Code);
                if (paymentTerm != null)
                {
                    return BadRequest(new { message = "Payment Term already exists" });
                }
                var createPaymentTermServiceModel = _mapper.Map<CreatePaymentTermServiceModel>(createPaymentTermAPIModel);

                var addedPaymentTerm = await _paymentTermService.AddPaymentTermAsync(createPaymentTermServiceModel);
                var paymentTermAPIModel = _mapper.Map<CreatePaymentTermAPIModel>(addedPaymentTerm);
                return CreatedAtRoute(nameof(GetPaymentTermByCodeAsync), new { code = paymentTermAPIModel.Code }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("list/{code}", Name = "GetPaymentTermByCodeAsync")]
        [Authorize(Policy = "payment-term-view")]
        [ProducesResponseType(typeof(PaymentTermAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Payment Term Endpoints" },
             Summary = "Payment Term details for a given Payment Term Code",
             Description = "Returns 200 - OK with Payment Term model.")
         ]
        public async Task<IActionResult> GetPaymentTermByCodeAsync(string code)
        {
            try
            {
                var paymentTermAPIModel = _mapper.Map<PaymentTermAPIModel>(await _paymentTermService.GetPaymentTermByCodeAsync(code));
                if (paymentTermAPIModel == null)
                {
                    return UnprocessableEntity("Payment Term is not available for code :" + code);
                }
                return Ok(paymentTermAPIModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Policy = "payment-term-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Payment Term Endpoints" },
           Summary = "Delete a Payment Term.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeletePaymentTermAsync(string code)
        {
            var paymentTerm = await _paymentTermService.GetPaymentTermByCodeAsync(code);
            if (paymentTerm == null)
            {
                return UnprocessableEntity("Payment Term is not available for code :" + code);
            }
            await _paymentTermService.DeletePaymentTermAsync(code);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Policy = "payment-term-manage")]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Payment Term Endpoints" },
           Summary = "Update a Payment Term.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> UpdatePaymentTermAsync([FromQuery] string code, [FromBody] UpdatePaymentTermAPIModel
           updatePaymentTermAPIModel)
        {
            try
            {
                var resultPaymentTermAPIModel = _mapper.Map<PaymentTermAPIModel>(await _paymentTermService.GetPaymentTermByCodeAsync(code));
                if (resultPaymentTermAPIModel == null)
                {
                    return UnprocessableEntity("payment term is not available for code :" + code);
                }
                updatePaymentTermAPIModel.Code = resultPaymentTermAPIModel.Code;
                var updatePaymentTermServiceModel = _mapper.Map<UpdatePaymentTermServiceModel>(updatePaymentTermAPIModel);
                await _paymentTermService.UpdatePaymentTermAsync(updatePaymentTermServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
