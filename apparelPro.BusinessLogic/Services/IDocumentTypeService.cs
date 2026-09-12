using apparelPro.BusinessLogic.Services.Models.ImportExport.IDocumentTypeService;
using ApparelPro.Shared.Extensions;

namespace apparelPro.BusinessLogic.Services
{
    public interface IDocumentTypeService
    {
        Task<PaginationResult<DocumentTypeServiceModel>> GetDocumentTypesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery);
        Task<DocumentTypeServiceModel> GetDocumentTypeByIdAsync(int id);
        Task<DocumentTypeServiceModel> GetDocumentTypeByDocNoAndDocTypeCodeAsync(string docNo, string docTypeCode);
        Task<DocumentTypeServiceModel> AddDocumentTypeAsync(CreateDocumentTypeServiceModel createDocumentTypeServiceModel);
        Task UpdateDocumentTypeAsync(UpdateDocumentTypeServiceModel updateDocumentTypeServiceModel);
        Task DeleteDocumentTypeAsync(int id);
    }
}
