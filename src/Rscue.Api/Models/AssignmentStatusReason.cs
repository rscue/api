namespace Rscue.Api.Models
{
    public enum AssignmentStatusReason
    {
        New = 1,
        CancelledByUser = 2,
        CancelledByProvider = 3,
        CancelledByWorker = 4,
        WorkerAssigned = 5,
        WorkerEnRoute = 6,
        ServiceCompleted = 7,
        ClosedBySystem = 8
    }
}
