namespace Rscue.Api.Tests
{
    using Rscue.Api.Models;
    using System;
    using System.Linq.Expressions;

    public interface ITestDataStore
    {
        void EnsureNoAssignments();
        void EnsureNoClients();
        void EnsureProvider(Provider provider);
        void EnsureClient(Client client);
        void EnsureAssignment(Assignment assignment);
        void EnsureWorker(ProviderWorker worker);
        void EnsureImageBucket(ImageBucket imageBucket);
        void EnsureProviderBoatTow(ProviderBoatTow providerBoatTow);

        void EnsureImageBucketDoesNotExist(ImageBucketKey imageBucketKey);
        void EnsureProviderDoesNotExist(string providerId);
        void EnsureClientDoesNotExist(string clientId);
        void EnsureAssignmentDoesNotExist(string assignmentId);
        void EnsureWorkerDoesNotExist(string workerId);
        void EnsureProviderBoatTowDoesNotExist(string providerId, string id);

        bool TestImageBucket(Expression<Func<ImageBucket, bool>> filter);
        bool TestProvider(Expression<Func<Provider, bool>> filter);
        bool TestProviderBoatTow(Expression<Func<ProviderBoatTow, bool>> filter);
    }
}
