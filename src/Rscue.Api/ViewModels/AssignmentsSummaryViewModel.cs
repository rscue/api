namespace Rscue.Api.ViewModels
{
    public class AssignmentsSummaryViewModel
    {
        public long Created { get; set; }
        public long InProgress { get; set; }
        public long Completed { get; set; }
        public long Cancelled { get; set; }
    }
}