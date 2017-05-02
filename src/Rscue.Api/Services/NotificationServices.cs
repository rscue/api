namespace Rscue.Api.Services
{
    using Microsoft.Extensions.Options;
    using MongoDB.Driver;
    using Rscue.Api.Models;
    using Rscue.Api.Plumbing;
    using System.Threading.Tasks;

    public class NotificationServices : INotificationServices
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IOptions<ProviderAppSettings> _providerAppSettings;

        public NotificationServices(IMongoDatabase mongoDatabase, IOptions<ProviderAppSettings> providerAppSettings)
        {
            _mongoDatabase = mongoDatabase;
            _providerAppSettings = providerAppSettings;
        }

        public async Task<NotificationOutcome> NotifyAssignmentWorkerAsync(Assignment assignment)
        {
            try
            {
                var settings = _providerAppSettings.Value;
                var payload = PushNotificationHelpers.GetNewAssignmentPayload(assignment.Worker.DeviceId, assignment.Id);
                await PushNotificationHelpers.Send(settings.ApplicationId, settings.SenderId, payload);
                return NotificationOutcome.Sent;
            }
            catch
            {
                return NotificationOutcome.Failed;
            }
        }
    }
}
