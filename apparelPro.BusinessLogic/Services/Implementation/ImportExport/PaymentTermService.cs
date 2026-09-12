using apparelPro.BusinessLogic.Misc;
using apparelPro.BusinessLogic.Services.Models.ImportExport.IPaymentTermService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Shared.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    public class PaymentTermService : IPaymentTermService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;
        public PaymentTermService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<PaymentTermServiceModel> AddPaymentTermAsync(CreatePaymentTermServiceModel createPaymentTermServiceModel)
        {
            try
            {
                var paymentTermDbModel = _mapper.Map<PaymentTerm>(createPaymentTermServiceModel);
                _apparelProDbContext.PaymentTerms.Add(paymentTermDbModel);
                await _apparelProDbContext.SaveChangesAsync();
                return _mapper.Map<PaymentTermServiceModel>(paymentTermDbModel);
            }
            catch (Exception ex)
            {
                throw new Exception("Payment Term already exists");
            }
        }

        public async Task DeletePaymentTermAsync(string code)
        {
            try
            {
                var paymentTermDbModel = await _apparelProDbContext.PaymentTerms
                    .Where(paymentTerm => paymentTerm.Code == code)
                    .FirstOrDefaultAsync();
                _apparelProDbContext.PaymentTerms.Remove(paymentTermDbModel!);
                await _apparelProDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResult<PaymentTermServiceModel>> GetPaymentTermsAsync(int pageNumber, int pageSize, string? sortColumn, string? sortOrder, string? filterColumn, string? filterQuery)
        {
            IQueryable<PaymentTerm> paymentTermPagination = _apparelProDbContext.PaymentTerms.AsNoTracking();

            FilterResult fr = new();
            fr.searchPattern = "{0}.Contains(@0)";
            fr.FilterColumn = filterColumn;
            fr.FilterQuery = filterQuery;
            if (filterColumn != null && filterQuery != null)
            {
                fr = InputValidator.Validate(filterColumn!, filterQuery!, typeof(PaymentTerm));
                paymentTermPagination = paymentTermPagination.Where(string.Format(fr.searchPattern!, fr.FilterColumn), fr.FilterQuery);
            }

            int counter = 0;
            counter = await paymentTermPagination.CountAsync();

            if (sortColumn != null)
            {
                sortOrder = !string.IsNullOrEmpty(sortOrder) && sortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";
                paymentTermPagination = paymentTermPagination.OrderBy(string.Format("{0} {1}", sortColumn, sortOrder));
            }
            paymentTermPagination = paymentTermPagination
                .Skip(pageSize * pageNumber)
                .Take(pageSize);

            var filteredDbPaymentTerms = await paymentTermPagination.ToListAsync();
            var paymentTermServiceModels = _mapper.Map<IList<PaymentTermServiceModel>>(filteredDbPaymentTerms);

            return new PaginationResult<PaymentTermServiceModel>(pageSize, pageNumber, counter, paymentTermServiceModels,
                sortColumn, sortOrder, filterColumn, filterQuery);
        }

        public async Task UpdatePaymentTermAsync(UpdatePaymentTermServiceModel updatePaymentTermServiceModel)
        {
            try
            {
                var paymentTermDbModel = await _apparelProDbContext.PaymentTerms
                    .Where(paymentTerm => paymentTerm.Code == updatePaymentTermServiceModel.Code)
                    .FirstOrDefaultAsync();
                if (paymentTermDbModel != null)
                {
                    paymentTermDbModel.Description = updatePaymentTermServiceModel.Description;
                    _apparelProDbContext.PaymentTerms.Update(paymentTermDbModel!);
                    await _apparelProDbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaymentTermServiceModel> GetPaymentTermByCodeAsync(string code)
        {
            var paymentTermDbModel = await _apparelProDbContext.PaymentTerms.Where(paymentTerm => paymentTerm.Code == code)
                .FirstOrDefaultAsync();
            var paymentTermServiceModel = _mapper.Map<PaymentTermServiceModel>(paymentTermDbModel);
            return paymentTermServiceModel;
        }
    }
}
