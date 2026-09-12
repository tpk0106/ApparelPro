using apparelPro.BusinessLogic.Services;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IDocumentTypeService;
using ApparelPro.WebApi.APIModels;
using ApparelPro.WebApi.APIModels.ImportExport;
using ApparelPro.WebApi.Misc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApparelPro.WebApi.Controllers.ImportExport
{
    [Route("api/document-type")]
    [ApiController]
    public class DocumentTypeController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IDocumentTypeService _documentTypeService;
        public DocumentTypeController(IMapper mapper, IDocumentTypeService documentTypeService)
        {
            _mapper = mapper;
            _documentTypeService = documentTypeService;
        }

        [HttpGet("list")]
        [Authorize(Policy = "document-type-view")]
        [ProducesResponseType(HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(PaginationAPIModel<DocumentTypeAPIModel>), HttpStatusCodes.OK)]
        [SwaggerOperation(Tags = new[] { "Document Type Endpoints" },
           Summary = "Document Type list.",
           Description = "Returns 200 - OK with list")
       ]
        public async Task<IActionResult> GetDocumentTypesAsync(
            [FromQuery] int pageSize,
            [FromQuery] int pageNumber,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortOrder = null,
            [FromQuery] string? filterColumn = null,
            [FromQuery] string? filterQuery = null
        )
        {
            var documentTypeServiceModels = await _documentTypeService.GetDocumentTypesAsync(pageNumber, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
            var documentTypes = _mapper.Map<PaginationAPIModel<DocumentTypeAPIModel>>(documentTypeServiceModels);
            return Ok(documentTypes);
        }

        [HttpPost()]
        [Authorize(Policy = "document-type-manage")]
        [ProducesResponseType(HttpStatusCodes.Created)]
        [SwaggerOperation(Tags = new[] { "Document Type Endpoints" },
           Summary = "Add a Document Type.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> AddDocumentTypeAsync([FromBody] CreateDocumentTypeAPIModel createDocumentTypeAPIModel)
        {
            try
            {
                var documentType = await _documentTypeService.GetDocumentTypeByDocNoAndDocTypeCodeAsync(createDocumentTypeAPIModel.DocNo, createDocumentTypeAPIModel.DocTypeCode);
                if (documentType != null)
                {
                    return BadRequest(new { message = "Document Type already exists" });
                }
                var createDocumentTypeServiceModel = _mapper.Map<CreateDocumentTypeServiceModel>(createDocumentTypeAPIModel);

                var addedDocumentType = await _documentTypeService.AddDocumentTypeAsync(createDocumentTypeServiceModel);
                var documentTypeAPIModel = _mapper.Map<DocumentTypeAPIModel>(addedDocumentType);
                return CreatedAtRoute(nameof(GetDocumentTypeByIdAsync), new { id = documentTypeAPIModel.Id }, null);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [HttpGet("list/{id}", Name = "GetDocumentTypeByIdAsync")]
        [Authorize(Policy = "document-type-view")]
        [ProducesResponseType(typeof(DocumentTypeAPIModel), HttpStatusCodes.OK)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Document Type Endpoints" },
             Summary = "Document Type details for a given Document Type Id",
             Description = "Returns 200 - OK with Document Type model.")
         ]
        public async Task<IActionResult> GetDocumentTypeByIdAsync(int id)
        {
            try
            {
                var documentTypeAPIModel = _mapper.Map<DocumentTypeAPIModel>(await _documentTypeService.GetDocumentTypeByIdAsync(id));
                if (documentTypeAPIModel == null)
                {
                    return UnprocessableEntity("Document Type is not available for id :" + id);
                }
                return Ok(documentTypeAPIModel);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "document-type-manage")]
        [ProducesResponseType(HttpStatusCodes.NoContent)]
        [ProducesResponseType(typeof(UnprocessableEntityResult), HttpStatusCodes.UnprocessableEntity)]
        [SwaggerOperation(Tags = new[] { "Document Type Endpoints" },
           Summary = "Delete a Document Type.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> DeleteDocumentTypeAsync(int id)
        {
            var documentType = await _documentTypeService.GetDocumentTypeByIdAsync(id);
            if (documentType == null)
            {
                return UnprocessableEntity("Document Type is not available for id :" + id);
            }
            await _documentTypeService.DeleteDocumentTypeAsync(id);
            return NoContent();
        }

        [HttpPut()]
        [Authorize(Policy = "document-type-manage")]
        [ProducesResponseType(typeof(BadRequestResult), HttpStatusCodes.BadRequest)]
        [ProducesResponseType(typeof(void), HttpStatusCodes.NoContent)]
        [SwaggerOperation(Tags = new[] { "Document Type Endpoints" },
           Summary = "Update a Document Type.",
           Description = "Returns 200 - OK with No content")
       ]
        public async Task<IActionResult> UpdateDocumentTypeAsync([FromQuery] int id, [FromBody] UpdateDocumentTypeAPIModel
           updateDocumentTypeAPIModel)
        {
            try
            {
                var resultDocumentTypeAPIModel = _mapper.Map<DocumentTypeAPIModel>(await _documentTypeService.GetDocumentTypeByIdAsync(id));
                if (resultDocumentTypeAPIModel == null)
                {
                    return UnprocessableEntity("document type is not available for id :" + id);
                }
                updateDocumentTypeAPIModel.Id = resultDocumentTypeAPIModel.Id;
                var updateDocumentTypeServiceModel = _mapper.Map<UpdateDocumentTypeServiceModel>(updateDocumentTypeAPIModel);
                await _documentTypeService.UpdateDocumentTypeAsync(updateDocumentTypeServiceModel);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
