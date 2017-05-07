using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rscue.Api.Tests
{
    public static class MongoDbHelper
    {
        public static IMongoDatabase GetRscueCenterUnitTestDatabase()
        {
            var mongoUrl = "mongodb://rscue:AG4JgMNVpllX7vJF@unit-test-shard-00-00-5mmof.mongodb.net:27017,unit-test-shard-00-01-5mmof.mongodb.net:27017,unit-test-shard-00-02-5mmof.mongodb.net:27017/RscueCenter?ssl=true&replicaSet=unit-test-shard-0&authSource=admin";
            var mongoDatabase = "RscueCenter";
            var client = new MongoClient(mongoUrl);
            var database = client.GetDatabase(mongoDatabase);
            return database;
        }
    }
}
