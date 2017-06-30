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
            var mongoUrl = Environment.GetEnvironmentVariable("RSCUE_API_MongoDb__Url");
            var mongoDatabase = "RscueCenter";
            var client = new MongoClient(mongoUrl);
            var database = client.GetDatabase(mongoDatabase);
            return database;
        }
    }
}
