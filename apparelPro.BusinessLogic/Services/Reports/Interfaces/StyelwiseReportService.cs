using System;
using System.Collections.Generic;
using System.Text;

namespace apparelPro.BusinessLogic.Services.Reports.Interfaces
{
    public interface IStylewiseReportService
    {
        public Task PrintReport(int buyerCode, string order, int typeCode, string styleCode);
    }
}
