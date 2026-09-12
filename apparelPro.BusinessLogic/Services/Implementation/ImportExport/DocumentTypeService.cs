using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IDocumentTypeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        public DocumentTypeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<DocumentTypeServiceModel> AddDocumentTypeAsync(CreateDocumentTypeServiceModel createDocumentTypeServiceModel)
        {
            try
            {
                var documentTypeDbModel = _mapper.Map<DocumentType>(createDocumentTypeServiceModel);
                _apparelProDbContext.DocumentTypes.Add(documentTypeDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<DocumentTypeServiceModel>(documentTypeDbModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Document Type already exists");
            }
        }

        public async Task DeleteDocumentTypeAsync(int id)
        {
            try
            {
                var documentTypeDbModel = await _apparelProDbContext.DocumentTypes
                    .Where(documentType => documentType.Id == id)
                    .FirstOrDefaultAsync();
                _apparelProDbContext.DocumentTypes.Remove(documentTypeDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResult<DocumentTypeServiceModel>> GetDocumentTypesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<DocumentType> documentTypePagination = _apparelProDbContext.DocumentTypes.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(DocumentType));
                documentTypePagination = documentTypePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await documentTypePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                documentTypePagination = documentTypePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            documentTypePagination = documentTypePagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbDocumentTypes = await documentTypePagination.ToListAsync();
            var documentTypeServiceModels = _mapper.Map<IList<DocumentTypeServiceModel>>(filteredDbDocumentTypes);

            return new PaginationResult<DocumentTypeServiceModel>(pageSize, pageNumber, counter, documentTypeServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateDocumentTypeAsync(UpdateDocumentTypeServiceModel updateDocumentTypeServiceModel)
        {
            try
            {
                var documentTypeDbModel = await _apparelProDbContext.DocumentTypes
                    .Where(documentType => documentType.Id == updateDocumentTypeServiceModel.Id)
                    .FirstOrDefaultAsync();
                if (documentTypeDbModel != null)
                {
                    documentTypeDbModel.DocNo = updateDocumentTypeServiceModel.DocNo;
                    documentTypeDbModel.DocTypeCode = updateDocumentTypeServiceModel.DocTypeCode;
                    documentTypeDbModel.Description = updateDocumentTypeServiceModel.Description;
                    _apparelProDbContext.DocumentTypes.Update(documentTypeDbModel!);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<DocumentTypeServiceModel> GetDocumentTypeByIdAsync(int id)
        {
            var documentTypeDbModel = await _apparelProDbContext.DocumentTypes.Where(documentType => documentType.Id == id)
                .FirstOrDefaultAsync();
            var documentTypeServiceModel = _mapper.Map<DocumentTypeServiceModel>(documentTypeDbModel);
            return documentTypeServiceModel;
        }

        public async Task<DocumentTypeServiceModel> GetDocumentTypeByDocNoAndDocTypeCodeAsync(string docNo, string docTypeCode)
        {
            var documentTypeDbModel = await _apparelProDbContext.DocumentTypes
                .Where(documentType => documentType.DocNo == docNo && documentType.DocTypeCode == docTypeCode)
                .FirstOrDefaultAsync();
            var documentTypeServiceModel = _mapper.Map<DocumentTypeServiceModel>(documentTypeDbModel);
            return documentTypeServiceModel;
        }
    }
}
