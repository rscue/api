namespace Rscue.Api.Models
{
    public enum AssignmentStatusReason
    {
        New = 1,
        CancelledByUser = 2,
        CancelledByProvider = 3,
        WorkerAssigned = 4,
        WorkerArrivedAtIncidentLocation = 5,
        ServiceCompleted = 6,
        ClosedBySystem = 7
    }
}
