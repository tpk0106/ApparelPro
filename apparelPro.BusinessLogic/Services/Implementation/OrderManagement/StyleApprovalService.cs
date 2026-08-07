using apparelPro.BusinessLogic.Services.Models.OrderManagement.IStyleDetailsService;
using ApparelPro.Data;
using Microsoft.EntityFrameworkCore;

namespace apparelPro.BusinessLogic.Services.Implementation.OrderManagement
{
    public class StyleApprovalService:IStyleApprovalService
    {
        private readonly ApparelProDbContext _apparelProDbContext;

        public StyleApprovalService(ApparelProDbContext  apparelProDbContext)
        {
            _apparelProDbContext = apparelProDbContext;
        }

        public async Task<bool> ApproveStyleEventsAsync(int buyerCode, string order, int typeCode, string styleCode, string approvedByUserId, DateTime approvalDate)
        {
            order = order.Trim();
            styleCode = styleCode.Trim();
            approvedByUserId = approvedByUserId.Trim().ToUpper();

            // 1. Locate the master parent style tracking row in your database
            var styleRow = await _apparelProDbContext.Styles
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode && s.Order == order && s.TypeCode == typeCode && s.StyleCode == styleCode);

            if (styleRow == null)
            {
                throw new InvalidOperationException($"Style Selection Mismatch Error: Target style '{styleCode}' under order contract reference '{order}' was not discovered inside the master registry.");
            }

            // 2. THE CRITICAL RE-APPROVAL BLOCKER: Match Clipper's empty check to preserve data history trails
            // FIXED (2026-08-07): this was checking EstimateApprovalUserName, which this method
            // never writes (it writes Username/ApprovedDate - see the fix below and the matching
            // comment there from 2026-08-03). EstimateApprovalUserName sits permanently empty, so
            // this guard could never actually fire - re-approval (and silently overwriting the
            // original approver/date) was possible indefinitely. Legacy od_aprvl.prg's own guard
            // is "if !empty(userid) ... 'Trim Sheet already approved'" - Username is the correct
            // field to check here, matching od_style->userid exactly.
            if (!string.IsNullOrEmpty(styleRow.Username))
            {
                throw new InvalidOperationException("Trim Sheet already approved for this style.");
            }

            // 3. APPLY METADATA FOOTPRINT KEYS ATOMICALY
            // FIXED (2026-08-03): this action is Trim Sheet Approval (legacy od_aprvl.prg,
            // od_style->userid/aprv_date) - NOT the separate Style-wise Events Approval
            // (legacy od_evapr.prg, od_style->ea_userid/ea_date). Username/ApprovedDate are
            // the correct pair for this: StylewiseEventService already reads styleHeader.Username
            // expecting exactly this approver stamp. Previously this wrote the approver name to
            // EstimateApprovalUserName instead, so Username was never actually populated by this
            // action and any UI reading Username for "approved by" always showed blank.
            styleRow.Username = approvedByUserId;
            styleRow.ApprovedDate = DateOnly.FromDateTime(approvalDate);

            _apparelProDbContext.Styles.Update(styleRow);
            await _apparelProDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<StyleApprovalDetailsServiceModel?> GetStyleApprovalDetailsAsync(int buyerCode, string order, int typeCode, string styleCode)
        {
            // Query your master style headers table in SQL Server
            var styleHeader = await _apparelProDbContext.Styles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.BuyerCode == buyerCode &&
                                          s.Order == order.Trim() &&
                                          s.TypeCode == typeCode &&
                                          s.StyleCode == styleCode.Trim());

            // FIXED VERIFICATION CONDITIONAL: Verify if an approval signature matches 
            if (styleHeader != null && styleHeader.ApprovedDate.HasValue &&
                styleHeader.ApprovedDate.Value != DateOnly.FromDateTime(DateTime.MinValue))
            {
                // Return the payload data structure straight to the controller layer.
                // Reads Username (see the matching fix in ApproveStyleEventsAsync above) -
                // not EstimateApprovalUserName, which is a different legacy flag entirely.
                return new StyleApprovalDetailsServiceModel
                {
                    EstimateApprovalUserName = styleHeader.Username ?? "SYSTEM_ADMIN",
                    EstimateApprovalDate = styleHeader.ApprovedDate.Value
                };
            }

            // Return null to signal the controller that the material sheet is completely unlocked!
            return null;
        }
    }
}
