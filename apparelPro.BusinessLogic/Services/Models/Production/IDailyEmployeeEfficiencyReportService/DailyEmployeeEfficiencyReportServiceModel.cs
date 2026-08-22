namespace apparelPro.BusinessLogic.Services.Models.Production.IDailyEmployeeEfficiencyReportService
{
    // Replicates PR_EEF1.PRG's "DAILY EMPLOYEE EFFICIENCY" report (Reports ->
    // Employee Efficiency (Daily)). Reuses the already-ported
    // DailyProductionEfficiencyCalculator (apparelPro.BusinessLogic.Production)
    // - the same math the Daily Production Time Ticket screen uses for a
    // single style/ticket - just fed with every DailyProductionTimeTicketEntry
    // for the given Date (+ optional Line), which can span multiple styles.
    public class EmployeeEfficiencyRowServiceModel
    {
        public string EmployeeCode { get; set; } = null!;
        public string EmployeeName { get; set; } = "";
        public decimal WorkHours { get; set; }
        public decimal EarnedHours { get; set; }
        public decimal NonProductiveHours { get; set; }
        public decimal OverEfficiencyPercent { get; set; }
        public decimal OperatorEfficiencyPercent { get; set; }
    }

    public class DailyEmployeeEfficiencyReportServiceModel
    {
        public DateOnly Date { get; set; }
        public string? LineCode { get; set; }
        public string? LineDescription { get; set; }
        public List<EmployeeEfficiencyRowServiceModel> Rows { get; set; } = new();
    }
}
