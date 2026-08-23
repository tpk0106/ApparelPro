using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.Reference.IPortDestinationService;
using ApparelPro.Data;
using ApparelPro.Data.Models.References;
using ApparelPro.Shared.Extensions;
using ApparelPro.Shared.LookupConstants;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.Reference
{
    public class PortDestinationService : IPortDestinationService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        private readonly ILookupConstants _lookupConstants;
        private readonly IDistributedCache _distributedCache;
        public PortDestinationService(IMapper mapper, ApparelProDbContext apparelProDbContext) 
        {           
            if (apparelProDbContext == null)
            {
                throw new ArgumentNullException(nameof(apparelProDbContext));
            }
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaginationResult<PortDestinationServiceModel>> GetPortDestinationsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            var portDestinationWithCountriesPagination =  _apparelProDbContext.Destinations
                .Join(_apparelProDbContext.Countries,
                destination => destination.CountryCode, 
                country => country.Code, 
                (destination, country) => new { destination, country })
                .AsNoTracking().Select(a => new Destination {
                    Code = a.destination.Code,
                    CountryCode = a.destination.CountryCode,
                    CountryName = a.country.Name, 
                    DestinationName = a.destination.DestinationName
                })
                .AsQueryable();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(Destination));
                portDestinationWithCountriesPagination = portDestinationWithCountriesPagination
                    .Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await portDestinationWithCountriesPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                portDestinationWithCountriesPagination = portDestinationWithCountriesPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }

            portDestinationWithCountriesPagination = portDestinationWithCountriesPagination
                   .Skip(pageSize * pageNumber)
                   .Take(pageSize);

            var filteredDbCountries = await portDestinationWithCountriesPagination.ToListAsync();
            var portDestinationrServiceModels = _mapper.Map<IList<PortDestinationServiceModel>>(filteredDbCountries);

            return new PaginationResult<PortDestinationServiceModel>(pageSize, pageNumber, counter, portDestinationrServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task<PortDestinationServiceModel> GetPortDestinationByCodeAndCountryCodeAsync(string code, string countryCode)
        {
            var portDestinationDbModel = await _apparelProDbContext.Destinations
               .Where(portDestination => portDestination.Code == code && portDestination.CountryCode == countryCode)
               .FirstOrDefaultAsync();
            var portDestinationServiceModel = _mapper.Map<PortDestinationServiceModel>(portDestinationDbModel);
            return portDestinationServiceModel;
        }

        public async Task<PortDestinationServiceModel> AddPortDestinationAsync(CreatePortDestinationServiceModel createPortDestinationServiceModel)
        {
            var portDestinationDbModel = _mapper.Map<Destination>(createPortDestinationServiceModel);
            _apparelProDbContext.Destinations.Add(portDestinationDbModel);
            await _apparelProDbContext.SaveChangesAsync();
            return _mapper.Map<PortDestinationServiceModel>(portDestinationDbModel);
        }

        public async Task UpdatePortDestinationAsync(UpdatePortDestinationServiceModel updatePortDestinationServiceModel)
        {
            var portDestinatonDbModel = await _apparelProDbContext.Destinations
             .Where(portDestination => portDestination.Code == updatePortDestinationServiceModel.Code &&
                portDestination.CountryCode == updatePortDestinationServiceModel.CountryCode)
             .FirstOrDefaultAsync();

            portDestinatonDbModel!.DestinationName = updatePortDestinationServiceModel.DestinationName;
           
            _apparelProDbContext.Update(portDestinatonDbModel);
            await _apparelProDbContext.SaveChangesAsync();
        }

        public async Task DeletePortDestinationAsync(string code, string countryCode)
        {
            var portDestinatonDbModel = await _apparelProDbContext.Destinations
              .Where(portDestination => portDestination.CountryCode == countryCode && portDestination.Code == code)
              .FirstOrDefaultAsync();
            _apparelProDbContext.Destinations.Remove(portDestinatonDbModel!);
            await _apparelProDbContext.SaveChangesAsync();
        }
    }
}
