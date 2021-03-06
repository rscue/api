﻿namespace Rscue.Api.Tests
{
    using Rscue.Api.Extensions;
    using System;
    using Rscue.Api.Models;
    using MongoDB.Driver;
    using System.Linq.Expressions;

    public class MongoTestDataStore : ITestDataStore
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoTestDataStore(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase?? throw new ArgumentNullException(nameof(mongoDatabase));
        }

        public void EnsureAssignment(Assignment assignment)
        {
            _mongoDatabase.Assignments().InsertOne(assignment);
        }

        public void EnsureAssignmentDoesNotExist(string assignmentId)
        {
            _mongoDatabase.Assignments().DeleteOne(_ => _.Id == assignmentId);
        }

        public void EnsureClient(Client client)
        {
            _mongoDatabase.Clients().InsertOne(client);
        }

        public void EnsureClientDoesNotExist(string clientId)
        {
            _mongoDatabase.Clients().DeleteOne(_ => _.Id == clientId);
        }

        public void EnsureImageBucket(ImageBucket imageBucket)
        {
            _mongoDatabase.ImageBuckets().InsertOne(imageBucket);
        }

        public void EnsureImageBucketDoesNotExist(ImageBucketKey imageBucketKey)
        {
            _mongoDatabase.ImageBuckets().DeleteOne(_ => _.StoreBucket.Store == imageBucketKey.Store && _.StoreBucket.Bucket == imageBucketKey.Bucket);
        }

        public void EnsureNoAssignments()
        {
            _mongoDatabase.Assignments().DeleteMany(_ => true);
        }

        public void EnsureNoClients()
        {
            _mongoDatabase.Clients().DeleteMany(_ => true);
        }

        public void EnsureProvider(Provider provider)
        {
            _mongoDatabase.Providers().InsertOne(provider);
        }

        public void EnsureProviderBoatTow(ProviderBoatTow providerBoatTow)
        {
            _mongoDatabase.BoatTows().InsertOne(providerBoatTow);
        }

        public void EnsureProviderBoatTowDoesNotExist(string providerId, string id)
        {
            _mongoDatabase.BoatTows().DeleteOne(_ => _.Id == id && _.ProviderId == id);
        }

        public void EnsureProviderDoesNotExist(string providerId)
        {
            _mongoDatabase.Providers().DeleteOne(_ => _.Id == providerId);
        }

        public void EnsureProviderWorker(ProviderWorker providerWorker)
        {
            _mongoDatabase.Workers().InsertOne(providerWorker);
        }

        public void EnsureProviderWorkerDoesNotExist(string providerId, string id)
        {
            _mongoDatabase.Workers().DeleteOne(_ => _.Id == id && _.ProviderId == id);
        }

        public void EnsureWorker(ProviderWorker worker)
        {
            _mongoDatabase.Workers().InsertOne(worker);
        }

        public void EnsureWorkerDoesNotExist(string workerId)
        {
            _mongoDatabase.Workers().DeleteOne(_ => _.Id == workerId);
        }

        public bool TestImageBucket(Expression<Func<ImageBucket, bool>> filter)
        {
            return _mongoDatabase.ImageBuckets().Find(filter).SingleOrDefault() != null;
        }

        public bool TestProvider(Expression<Func<Provider, bool>> filter)
        {
            return _mongoDatabase.Providers().Find(filter).SingleOrDefault() != null;
        }

        public bool TestProviderBoatTow(Expression<Func<ProviderBoatTow, bool>> filter)
        {
            return _mongoDatabase.BoatTows().Find(filter).SingleOrDefault() != null;
        }

        public bool TestProviderWorker(Expression<Func<ProviderWorker, bool>> filter)
        {
            return _mongoDatabase.Workers().Find(filter).SingleOrDefault() != null;
        }
    }
}
