using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.ITransportModeService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    public class TransportModeService : ITransportModeService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        public TransportModeService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<TransportModeServiceModel> AddTransportModeAsync(CreateTransportModeServiceModel createTransportModeServiceModel)
        {
            try
            {
                var transportModeDbModel = _mapper.Map<TransportMode>(createTransportModeServiceModel);
                _apparelProDbContext.TransportModes.Add(transportModeDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<TransportModeServiceModel>(transportModeDbModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Transport Mode already exists");
            }
        }

        public async Task DeleteTransportModeAsync(string code)
        {
            try
            {
                var transportModeDbModel = await _apparelProDbContext.TransportModes
                    .Where(transportMode => transportMode.Code == code)
                    .FirstOrDefaultAsync();
                _apparelProDbContext.TransportModes.Remove(transportModeDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResult<TransportModeServiceModel>> GetTransportModesAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<TransportMode> transportModePagination = _apparelProDbContext.TransportModes.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(TransportMode));
                transportModePagination = transportModePagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await transportModePagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                transportModePagination = transportModePagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            transportModePagination = transportModePagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbTransportModes = await transportModePagination.ToListAsync();
            var transportModeServiceModels = _mapper.Map<IList<TransportModeServiceModel>>(filteredDbTransportModes);

            return new PaginationResult<TransportModeServiceModel>(pageSize, pageNumber, counter, transportModeServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdateTransportModeAsync(UpdateTransportModeServiceModel updateTransportModeServiceModel)
        {
            try
            {
                var transportModeDbModel = await _apparelProDbContext.TransportModes
                    .Where(transportMode => transportMode.Code == updateTransportModeServiceModel.Code)
                    .FirstOrDefaultAsync();
                if (transportModeDbModel != null)
                {
                    transportModeDbModel.Description = updateTransportModeServiceModel.Description;
                    _apparelProDbContext.TransportModes.Update(transportModeDbModel!);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<TransportModeServiceModel> GetTransportModeByCodeAsync(string code)
        {
            var transportModeDbModel = await _apparelProDbContext.TransportModes.Where(transportMode => transportMode.Code == code)
                .FirstOrDefaultAsync();
            var transportModeServiceModel = _mapper.Map<TransportModeServiceModel>(transportModeDbModel);
            return transportModeServiceModel;
        }
    }
}
