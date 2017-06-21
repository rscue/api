namespace Rscue.Api
{
    using Rscue.Api.Controllers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class Constants
    {
        public const string ASSIGNMENT_IMAGES_STORE = "assignment-images";
        public const string PROVIDER_IMAGES_STORE = "provider-images";
        public const string CLIENT_IMAGES_STORE = "client-images";
        public const string WORKER_IMAGES_STORE = "worker-images";

        public static class Routes
        {
            public const string GET_IMAGE = nameof(ImagesController.GetImage);

            public const string GET_IMAGES = nameof(ImagesController.GetImages);

            public const string GET_ASSIGNMENT = nameof(AssignmentController.GetAssignment);

            public const string GET_PROVIDER = nameof(ProviderController.GetProvider);

            public const string GET_PROVIDER_BOATTOW = nameof(ProviderBoatTowController.GetProviderBoatTow);

            public const string GET_PROVIDER_BOATTOWS = nameof(ProviderBoatTowController.GetProviderBoatTows);

            public const string GET_PROVIDER_WORKER = nameof(ProviderWorkerController.GetWorker);

            public const string GET_PROVIDER_WORKERS = nameof(ProviderWorkerController.GetWorkers);
        }
    }
}
