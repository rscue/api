namespace Rscue.Api.Services
{
    using Rscue.Api.Models;
    using System.Threading.Tasks;

    public interface INotificationServices
    {
        Task<NotificationOutcome> NotifyAssignmentWorkerAsync(Assignment assignment);
    }
}
