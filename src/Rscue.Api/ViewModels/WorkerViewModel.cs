namespace Rscue.Api.ViewModels
{
    using Rscue.Api.Models;

    public class WorkerViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string ProfilePictureUrl { get; set; }

        public string DeviceId { get; set; }

        public string Password { get; set; }

        public GeoLocation LastKnownLocation { get; set; }

        public ProviderWorkerStatus Status { get; set; }
    }
}