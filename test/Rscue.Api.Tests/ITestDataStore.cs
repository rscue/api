namespace Rscue.Api.Tests
{
    using Rscue.Api.Models;
    using System;

    public interface ITestDataStore
    {
        void EnsureNoAssignments();
        void EnsureNoClients();
        void EnsureProvider(Provider provider);
        void EnsureClient(Client client);
        void EnsureAssignment(Assignment assignment);
        void EnsureWorker(Worker worker);

        void EnsureProviderDoesNotExist(string providerId);
        void EnsureClientDoesNotExist(string clientId);
        void EnsureAssignmentDoesNotExist(string assignmentId);
        void EnsureWorkerDoesNotExist(string workerId);
    }
}
