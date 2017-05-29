namespace Rscue.Api.Tests.Services
{
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ProviderBoatTowRepository")]
    [Trait("DependsOn", "mongodb")]
    public class ProviderBoatTowRepositoryTests
    {
        private readonly IProviderBoatTowRepository _boatTowRepository;
        private ITestDataStore _dataStore;

        public ProviderBoatTowRepositoryTests()
        {
            var mongoDatabase = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            _boatTowRepository = new ProviderBoatTowRepository(mongoDatabase);
            _dataStore = new MongoTestDataStore(mongoDatabase);
        }

    }
}
