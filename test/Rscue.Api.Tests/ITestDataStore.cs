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
        void EnsureWorker(Worker worker);
        void EnsureImageBucket(ImageBucket imageBucket);

        void EnsureImageBucketDoesNotExist(ImageBucketKey imageBucketKey);
        void EnsureProviderDoesNotExist(string providerId);
        void EnsureClientDoesNotExist(string clientId);
        void EnsureAssignmentDoesNotExist(string assignmentId);
        void EnsureWorkerDoesNotExist(string workerId);

        bool TestImageBucket(Expression<Func<ImageBucket, bool>> predicate);
        bool TestProvider(Expression<Func<Provider, bool>> predicate);
    }
}
