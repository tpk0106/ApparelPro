namespace ApparelPro.WebApi.APIModels.Dashboard
{
    public class ApprovalStageDetailAPIModel
    {
        public bool IsApproved { get; set; }
        public string? ApprovedBy { get; set; }
        public DateOnly? ApprovedDate { get; set; }
    }
}
