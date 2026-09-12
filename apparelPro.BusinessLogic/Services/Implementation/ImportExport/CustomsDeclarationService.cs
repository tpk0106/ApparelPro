using apparelPro.BusinessLogic.Services.Models.ImportExport.ICustomsDeclarationService;
using ApparelPro.Data;
using ApparelPro.Data.Models.ImportExport;
using ApparelPro.Data.Models.References;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.ImportExport
{
    // CRUD for CUSDEC I/II (legacy ie_cusd1-4, keyed by CusNo) - header +
    // item lines (each with its own tax sub-lines) + attached documents,
    // all saved together in one transaction, replace-the-child-sets-on-save
    // shape (same as LetterOfCreditService/CommercialInvoiceService).
    public class CustomsDeclarationService : ICustomsDeclarationService
    {
        private readonly IMapper _mapper;
        private readonly ApparelProDbContext _apparelProDbContext;

        public CustomsDeclarationService(IMapper mapper, ApparelProDbContext apparelProDbContext)
        {
            _mapper = mapper;
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<CustomsDeclarationDetailServiceModel?> GetByCusNoAsync(string cusNo)
        {
            var header = await _apparelProDbContext.CustomsDeclarationHeaders
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.CusNo == cusNo);
            if (header == null) return null;

            var lines = await _apparelProDbContext.CustomsDeclarationLines
                .AsNoTracking()
                .Where(l => l.CusNo == cusNo)
                .ToListAsync();

            var taxes = await _apparelProDbContext.CustomsDeclarationLineTaxes
                .AsNoTracking()
                .Where(t => t.CusNo == cusNo)
                .ToListAsync();

            var documents = await _apparelProDbContext.CustomsDeclarationAttachedDocuments
                .AsNoTracking()
                .Where(d => d.CusNo == cusNo)
                .ToListAsync();

            var lineModels = _mapper.Map<List<CustomsDeclarationLineServiceModel>>(lines);
            foreach (var lineModel in lineModels)
            {
                lineModel.Taxes = _mapper.Map<List<CustomsDeclarationLineTaxServiceModel>>(
                    taxes.Where(t => t.Item == lineModel.Item));
            }

            return new CustomsDeclarationDetailServiceModel
            {
                Header = _mapper.Map<CustomsDeclarationHeaderServiceModel>(header),
                Lines = lineModels,
                AttachedDocuments = _mapper.Map<List<CustomsDeclarationAttachedDocumentServiceModel>>(documents),
            };
        }

        public async Task<CustomsDeclarationDetailServiceModel> SaveAsync(SaveCustomsDeclarationServiceModel serviceModel)
        {
            var cusNo = serviceModel.Header.CusNo;
            using var transaction = await _apparelProDbContext.Database.BeginTransactionAsync();

            var header = await _apparelProDbContext.CustomsDeclarationHeaders
                .FirstOrDefaultAsync(h => h.CusNo == cusNo);
            if (header == null)
            {
                header = _mapper.Map<CustomsDeclarationHeader>(serviceModel.Header);
                _apparelProDbContext.CustomsDeclarationHeaders.Add(header);
            }
            else
            {
                _mapper.Map(serviceModel.Header, header);
            }

            var existingTaxes = await _apparelProDbContext.CustomsDeclarationLineTaxes
                .Where(t => t.CusNo == cusNo)
                .ToListAsync();
            _apparelProDbContext.CustomsDeclarationLineTaxes.RemoveRange(existingTaxes);

            var existingLines = await _apparelProDbContext.CustomsDeclarationLines
                .Where(l => l.CusNo == cusNo)
                .ToListAsync();
            _apparelProDbContext.CustomsDeclarationLines.RemoveRange(existingLines);

            var existingDocuments = await _apparelProDbContext.CustomsDeclarationAttachedDocuments
                .Where(d => d.CusNo == cusNo)
                .ToListAsync();
            _apparelProDbContext.CustomsDeclarationAttachedDocuments.RemoveRange(existingDocuments);

            await _apparelProDbContext.SaveChangesAsync();

            var newLines = serviceModel.Lines.Select(l =>
            {
                var line = _mapper.Map<CustomsDeclarationLine>(l);
                line.Id = 0;
                line.CusNo = cusNo;
                return line;
            }).ToList();
            await _apparelProDbContext.CustomsDeclarationLines.AddRangeAsync(newLines);

            var newTaxes = serviceModel.Lines.SelectMany(l => l.Taxes.Select(t =>
            {
                var tax = _mapper.Map<CustomsDeclarationLineTax>(t);
                tax.Id = 0;
                tax.CusNo = cusNo;
                tax.Item = l.Item;
                return tax;
            })).ToList();
            await _apparelProDbContext.CustomsDeclarationLineTaxes.AddRangeAsync(newTaxes);

            var newDocuments = serviceModel.AttachedDocuments.Select(d =>
            {
                var document = _mapper.Map<CustomsDeclarationAttachedDocument>(d);
                document.Id = 0;
                document.CusNo = cusNo;
                return document;
            }).ToList();
            await _apparelProDbContext.CustomsDeclarationAttachedDocuments.AddRangeAsync(newDocuments);

            await _apparelProDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetByCusNoAsync(cusNo))!;
        }

        public async Task<CustomsDeclarationPrintDetailsServiceModel?> GetPrintDetailsAsync(string cusNo)
        {
            var header = await _apparelProDbContext.CustomsDeclarationHeaders
                .AsNoTracking().FirstOrDefaultAsync(h => h.CusNo == cusNo);
            if (header == null) return null;

            var lines = await _apparelProDbContext.CustomsDeclarationLines
                .AsNoTracking().Where(l => l.CusNo == cusNo).OrderBy(l => l.Item).ToListAsync();
            var taxes = await _apparelProDbContext.CustomsDeclarationLineTaxes
                .AsNoTracking().Where(t => t.CusNo == cusNo).ToListAsync();
            var documents = await _apparelProDbContext.CustomsDeclarationAttachedDocuments
                .AsNoTracking().Where(d => d.CusNo == cusNo).ToListAsync();

            var lineModels = _mapper.Map<List<CustomsDeclarationLineServiceModel>>(lines);
            foreach (var lineModel in lineModels)
            {
                lineModel.Taxes = _mapper.Map<List<CustomsDeclarationLineTaxServiceModel>>(
                    taxes.Where(t => t.Item == lineModel.Item));
            }

            static int ParseBuyerCode(string? code) => int.TryParse(code, out var n) ? n : 0;
            var buyerCodes = new[] { header.ExporterCode, header.ConsigneeCode, header.NotifyPartyCode, header.DeclarantCode }
                .Select(ParseBuyerCode).Where(c => c > 0).Distinct().ToList();
            var buyers = await _apparelProDbContext.Buyers.AsNoTracking()
                .Where(b => buyerCodes.Contains(b.BuyerCode)).ToDictionaryAsync(b => b.BuyerCode, b => b.Name);

            string BuyerNameFor(string? code) =>
                buyers.TryGetValue(ParseBuyerCode(code), out var name) ? name : "";

            var clearanceOffice = header.ClearanceOfficeCode == null ? null : await _apparelProDbContext.ClearanceOffices
                .AsNoTracking().FirstOrDefaultAsync(c => c.Code == header.ClearanceOfficeCode);
            var frontierOffice = header.FrontierOfficeCode == null ? null : await _apparelProDbContext.ClearanceOffices
                .AsNoTracking().FirstOrDefaultAsync(c => c.Code == header.FrontierOfficeCode);
            var paymentTerm = header.PaymentTermCode == null ? null : await _apparelProDbContext.PaymentTerms
                .AsNoTracking().FirstOrDefaultAsync(p => p.Code == header.PaymentTermCode);
            var deliveryTerm = header.DeliveryTermCode == null ? null : await _apparelProDbContext.Basis
                .AsNoTracking().FirstOrDefaultAsync(b => b.Code == header.DeliveryTermCode);
            var transportMode = header.TransportModeCode == null ? null : await _apparelProDbContext.TransportModes
                .AsNoTracking().FirstOrDefaultAsync(t => t.Code == header.TransportModeCode);
            var bank = header.BankCode == null ? null : await _apparelProDbContext.Banks
                .AsNoTracking().FirstOrDefaultAsync(b => b.BankCode == header.BankCode);

            var countryCodes = new[] { header.CountryOfConsignmentCode, header.CountryOfOriginCode, header.CountryOfDestinationCode }
                .Concat(lines.Select(l => l.CountryCode))
                .Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            var countries = await _apparelProDbContext.Countries.AsNoTracking()
                .Where(c => countryCodes.Contains(c.Code)).ToDictionaryAsync(c => c.Code, c => c.Name);
            string CountryNameFor(string? code) => code != null && countries.TryGetValue(code, out var name) ? name : "";

            var portCodes = new[] { header.PortOfLoadingCode, header.PortOfDischargeCode, header.PlaceOfDeliveryCode }
                .Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            var ports = await _apparelProDbContext.Destinations.AsNoTracking()
                .Where(p => portCodes.Contains(p.Code)).ToDictionaryAsync(p => p.Code, p => p.DestinationName);
            string PortNameFor(string? code) => code != null && ports.TryGetValue(code, out var name) ? name : "";

            var commodityCodes = lines.Select(l => l.CommodityCode).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            var commodities = await _apparelProDbContext.CommodityCodes.AsNoTracking()
                .Where(c => commodityCodes.Contains(c.Code)).ToDictionaryAsync(c => c.Code, c => c.Description);

            var cpcCodes = lines.Select(l => l.CustomsProcedureCode).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            var cpcs = await _apparelProDbContext.CustomsProcedureCodes.AsNoTracking()
                .Where(c => cpcCodes.Contains(c.Code)).ToDictionaryAsync(c => c.Code, c => c.Description);

            var agreementCodes = lines.Select(l => l.AgreementCode).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            var agreements = await _apparelProDbContext.AgreementCodes.AsNoTracking()
                .Where(c => agreementCodes.Contains(c.Code)).ToDictionaryAsync(c => c.Code, c => c.Description);

            var unitCodes = lines.Select(l => l.SupplementaryUnitCode).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            var units = await _apparelProDbContext.Units.AsNoTracking()
                .Where(u => unitCodes.Contains(u.Code)).ToDictionaryAsync(u => u.Code, u => u.Description);

            var taxCodes = taxes.Select(t => t.TaxCode).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            var taxDescriptions = await _apparelProDbContext.DutyTaxCodes.AsNoTracking()
                .Where(t => taxCodes.Contains(t.Code)).ToDictionaryAsync(t => t.Code, t => t.Description);

            var taxBaseCodes = taxes.Select(t => t.BaseCode).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
            var taxBaseDescriptions = await _apparelProDbContext.TaxBaseCodes.AsNoTracking()
                .Where(t => taxBaseCodes.Contains(t.Code)).ToDictionaryAsync(t => t.Code, t => t.Description);

            var docNos = documents.Select(d => d.DocNo).Distinct().ToList();
            var documentTypes = await _apparelProDbContext.DocumentTypes.AsNoTracking()
                .Where(d => docNos.Contains(d.DocNo)).ToDictionaryAsync(d => d.DocNo + "|" + d.DocTypeCode, d => d.Description);

            return new CustomsDeclarationPrintDetailsServiceModel
            {
                Header = _mapper.Map<CustomsDeclarationHeaderServiceModel>(header),
                Lines = lineModels,
                AttachedDocuments = _mapper.Map<List<CustomsDeclarationAttachedDocumentServiceModel>>(documents),
                ExporterName = BuyerNameFor(header.ExporterCode),
                ConsigneeName = BuyerNameFor(header.ConsigneeCode),
                NotifyPartyName = BuyerNameFor(header.NotifyPartyCode),
                DeclarantBuyerName = BuyerNameFor(header.DeclarantCode),
                ClearanceOfficeDescription = clearanceOffice?.Description ?? "",
                FrontierOfficeDescription = frontierOffice?.Description ?? "",
                PaymentTermDescription = paymentTerm?.Description ?? "",
                DeliveryTermDescription = deliveryTerm?.Description ?? "",
                TransportModeDescription = transportMode?.Description ?? "",
                CountryOfConsignmentName = CountryNameFor(header.CountryOfConsignmentCode),
                CountryOfOriginName = CountryNameFor(header.CountryOfOriginCode),
                CountryOfDestinationName = CountryNameFor(header.CountryOfDestinationCode),
                PortOfLoadingName = PortNameFor(header.PortOfLoadingCode),
                PortOfDischargeName = PortNameFor(header.PortOfDischargeCode),
                PlaceOfDeliveryName = PortNameFor(header.PlaceOfDeliveryCode),
                BankName = bank?.Name ?? "",
                CommodityDescriptions = commodities,
                CustomsProcedureDescriptions = cpcs,
                AgreementDescriptions = agreements,
                CountryNames = countries,
                UnitDescriptions = units,
                TaxDescriptions = taxDescriptions,
                TaxBaseDescriptions = taxBaseDescriptions,
                DocumentTypeDescriptions = documentTypes,
            };
        }

        public async Task<bool> DeleteAsync(string cusNo)
        {
            var header = await _apparelProDbContext.CustomsDeclarationHeaders
                .FirstOrDefaultAsync(h => h.CusNo == cusNo);
            if (header == null) return false;

            var taxes = await _apparelProDbContext.CustomsDeclarationLineTaxes
                .Where(t => t.CusNo == cusNo).ToListAsync();
            var lines = await _apparelProDbContext.CustomsDeclarationLines
                .Where(l => l.CusNo == cusNo).ToListAsync();
            var documents = await _apparelProDbContext.CustomsDeclarationAttachedDocuments
                .Where(d => d.CusNo == cusNo).ToListAsync();

            _apparelProDbContext.CustomsDeclarationLineTaxes.RemoveRange(taxes);
            _apparelProDbContext.CustomsDeclarationLines.RemoveRange(lines);
            _apparelProDbContext.CustomsDeclarationAttachedDocuments.RemoveRange(documents);
            _apparelProDbContext.CustomsDeclarationHeaders.Remove(header);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }
    }
}
