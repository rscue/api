namespace Rscue.Api.Tests.Mocks
{
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Threading.Tasks;

    public class NotificationServicesMock : INotificationServices
    {
        private Func<Assignment, NotificationOutcome> _assignmentMapping = _ => NotificationOutcome.Sent;

        public NotificationServicesMock(Func<Assignment, NotificationOutcome> assignmentMapping = null)
        {
            if (assignmentMapping != null)
            {
                _assignmentMapping = assignmentMapping;
            }
        }

        public Task<NotificationOutcome> NotifyAssignmentWorkerAsync(Assignment assignment) => Task.FromResult(_assignmentMapping(assignment));
    }
}
