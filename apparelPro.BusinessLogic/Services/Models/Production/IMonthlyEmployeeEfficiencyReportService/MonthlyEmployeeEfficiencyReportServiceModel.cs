namespace apparelPro.BusinessLogic.Services.Models.Production.IMonthlyEmployeeEfficiencyReportService
{
    // Replicates PR_REP4.PRG's "EMPLOYEE EFFICIENCY REPORT" (Reports ->
    // Employee Efficiency (Monthly)). One row per employee active in the
    // month, one column per calendar day showing Operator Efficiency % for
    // that day (null if the employee had no entries that day), plus a
    // trailing simple average across the days actually worked.
    //
    // Deliberate legacy quirk preserved: unlike the Daily report (which uses
    // each time-ticket entry's own WorkHours), this report uses the
    // factory-wide ProductionWorkHoursPerDay System Parameter as the
    // denominator for every employee/day - traced directly from
    // "m_wrkhrs = factpara->work_hrs" in PR_REP4.PRG, a different design
    // choice than its daily sibling, not a bug to "fix".
    public class EmployeeMonthlyEfficiencyDayCellServiceModel
    {
        public int Day { get; set; }
        public decimal? OperatorEfficiencyPercent { get; set; }
    }

    public class EmployeeMonthlyEfficiencyRowServiceModel
    {
        public string EmployeeCode { get; set; } = null!;
        public string EmployeeName { get; set; } = "";
        public List<EmployeeMonthlyEfficiencyDayCellServiceModel> Days { get; set; } = new();
        public decimal MonthlyAverageEfficiencyPercent { get; set; }
    }

    public class MonthlyEmployeeEfficiencyReportServiceModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int DaysInMonth { get; set; }
        public decimal WorkHoursPerDay { get; set; }
        public List<EmployeeMonthlyEfficiencyRowServiceModel> Rows { get; set; } = new();
    }
}
