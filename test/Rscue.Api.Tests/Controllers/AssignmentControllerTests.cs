namespace Rscue.Api.Tests.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Driver;
    using MongoDB.Driver.GeoJsonObjectModel;
    using Rscue.Api.Controllers;
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using Rscue.Api.Tests.Mocks;
    using Rscue.Api.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;
    using Rscue.Api.BindingModels;
    using Moq;
    using Microsoft.AspNetCore.Mvc.Routing;

    public class AssignmentControllerTests
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ITestDataStore _testDataStore;

        public AssignmentControllerTests()
        {
            _mongoDatabase = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            _testDataStore = new MongoTestDataStore(_mongoDatabase);
        }

        [Fact]
        public async Task TestGetAssignmentSucceedsWith200OnExistentDocument()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new ProviderWorker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignment =
                            new Assignment
                            {
                                Id = Guid.NewGuid().ToString("n"),
                                ClientId = client.Id,
                                WorkerId = worker.Id,
                                ProviderId = provider.Id,
                                Status = AssignmentStatus.Created,
                                StatusReason = AssignmentStatusReason.New,
                                CreationDateTime = DateTimeOffset.Now
                            };

            _testDataStore.EnsureClient(client);
            _testDataStore.EnsureProvider(provider);
            _testDataStore.EnsureWorker(worker);
            _testDataStore.EnsureAssignment(assignment);

            // act
            var actualResult = await assignmentController.GetAssignment(assignment.Id);

            // assert
            var actualAssignmentResult = actualResult as OkObjectResult;
            var actualAssignment = actualAssignmentResult.Value as AssignmentViewModel;
            Assert.True(actualAssignmentResult != null, "actualAssignmentResult should be of type OkObjectResult");
            Assert.True(actualAssignment != null, "actualAssignment should be of Type Assignment");
            Assert.Equal(200, actualAssignmentResult.StatusCode);
            Assert.Equal(assignment.Id, actualAssignment.Id);
        }

        [Fact]
        public async Task TestGetAssignmentFailsWith404OnNonExistentDocument()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var nonExistantId = "ffff0000ffff0000ffff0000";
            _testDataStore.EnsureAssignmentDoesNotExist(nonExistantId);

            // act
            var actualResult = await assignmentController.GetAssignment(nonExistantId);

            // assert
            var actualAssignmentResult = actualResult as NotFoundResult;
            Assert.True(actualAssignmentResult != null, "actualAssignmentResult should be of type NotFoundResult");
            Assert.Equal(404, actualAssignmentResult.StatusCode);
        }

        [Fact]
        public async Task TestNewAssignmentSucceedsWith201()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new ProviderWorker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignmentVM =
                            new NewAssignmentBindingModel
                            {
                                InitialLocation = new GeoLocation { Latitude = -34.605062, Longitude = -58.375979 },
                                ClientId = client.Id,
                                WorkerId = worker.Id,
                                ProviderId = provider.Id,
                                Status = AssignmentStatus.Created,
                                StatusReason = AssignmentStatusReason.New,
                                CreationDateTime = DateTimeOffset.Now
                            };

            _testDataStore.EnsureClient(client);
            _testDataStore.EnsureProvider(provider);
            _testDataStore.EnsureWorker(worker);

            // act
            var actualResult = await assignmentController.NewAssignment(assignmentVM);

            // assert
            var actualNewAssignmentResult = actualResult as CreatedResult;
            Assert.True(actualNewAssignmentResult != null, "actualNewAssignmentResult should be of type CreatedResult");
            Assert.Equal(201, actualNewAssignmentResult.StatusCode);
            Assert.Matches("^.*/assignment/.*$", actualNewAssignmentResult.Location);
        }

        [Fact]
        public async Task TestNewAssignmentFailsWith400OnMissingProvider()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var nonExistantProviderId = "ffff0000ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var worker = new ProviderWorker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignmentVM =
                            new NewAssignmentBindingModel
                            {
                                InitialLocation = new GeoLocation { Latitude = -34.605062, Longitude = -58.375979 },
                                ClientId = client.Id,
                                WorkerId = worker.Id,
                                ProviderId = nonExistantProviderId,
                                Status = AssignmentStatus.Created,
                                StatusReason = AssignmentStatusReason.New,
                                CreationDateTime = DateTimeOffset.Now
                            };

            _testDataStore.EnsureProviderDoesNotExist(nonExistantProviderId);
            _testDataStore.EnsureClient(client);
            _testDataStore.EnsureWorker(worker);

            // act
            var actualResult = await assignmentController.NewAssignment(assignmentVM);

            // assert
            var actualNewAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualNewAssignmentResult != null, "actualNewAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualNewAssignmentResult.StatusCode);
            Assert.NotNull(actualNewAssignmentResult.Value);
        }

        [Fact]
        public async Task TestNewAssignmentFailsWith400OnMissingWorker()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var nonExistantWorkerId = "ffff0000ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var assignmentVM =
                            new NewAssignmentBindingModel
                            {
                                InitialLocation = new GeoLocation { Latitude = -34.605062, Longitude = -58.375979 },
                                ClientId = client.Id,
                                WorkerId = nonExistantWorkerId,
                                ProviderId = provider.Id,
                                Status = AssignmentStatus.Created,
                                StatusReason = AssignmentStatusReason.New,
                                CreationDateTime = DateTimeOffset.Now
                            };

            _testDataStore.EnsureWorkerDoesNotExist(nonExistantWorkerId);
            _testDataStore.EnsureClient(client);
            _testDataStore.EnsureProvider(provider);

            // act
            var actualResult = await assignmentController.NewAssignment(assignmentVM);

            // assert
            var actualNewAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualNewAssignmentResult != null, "actualNewAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualNewAssignmentResult.StatusCode);
            Assert.NotNull(actualNewAssignmentResult.Value);
        }

        [Fact]
        public async Task TestNewAssignmentFailsWith400OnMissingClient()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var nonExistantClientId = "ffff0000ffff0000ffff0000ffff0000";
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new ProviderWorker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignmentVM =
                            new NewAssignmentBindingModel
                            {
                                InitialLocation = new GeoLocation { Latitude = -34.605062, Longitude = -58.375979 },
                                ClientId = nonExistantClientId,
                                WorkerId = worker.Id,
                                ProviderId = provider.Id,
                                Status = AssignmentStatus.Created,
                                StatusReason = AssignmentStatusReason.New,
                                CreationDateTime = DateTimeOffset.Now
                            };

            _testDataStore.EnsureClientDoesNotExist(nonExistantClientId);
            _testDataStore.EnsureProvider(provider);
            _testDataStore.EnsureWorker(worker);

            // act
            var actualResult = await assignmentController.NewAssignment(assignmentVM);

            // assert
            var actualNewAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualNewAssignmentResult != null, "actualNewAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualNewAssignmentResult.StatusCode);
            Assert.NotNull(actualNewAssignmentResult.Value);
        }

        [Fact]
        public async Task TestUpdateAssignmentSucceedsWith200()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new ProviderWorker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignment =
                new Assignment
                {
                    Id = Guid.NewGuid().ToString("n"),
                    Status = AssignmentStatus.Created,
                    StatusReason = AssignmentStatusReason.New,
                    CreationDateTime = DateTimeOffset.Now,
                    InitialLocation = new GeoJson2DGeographicCoordinates(-58.375979, -34.605062),
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            var updateAssignmentVM =
                new UpdateAssignmentBindingModel
                {
                    Status = AssignmentStatus.InProgress,
                    StatusReason = AssignmentStatusReason.WorkerEnRoute,
                    UpdateDateTime = DateTimeOffset.Now,
                    InitialLocation = new GeoLocation { Latitude = -34.605062, Longitude = -58.375979 },
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            _testDataStore.EnsureClient(client);
            _testDataStore.EnsureProvider(provider);
            _testDataStore.EnsureWorker(worker);
            _testDataStore.EnsureAssignment(assignment);

            // act
            var actualResult = await assignmentController.UpdateAssignment(assignment.Id, updateAssignmentVM);

            // assert
            var actualNewAssignmentResult = actualResult as OkObjectResult;
            Assert.True(actualNewAssignmentResult != null, "actualNewAssignmentResult should be of type OkObjectResult");
            Assert.Equal(200, actualNewAssignmentResult.StatusCode);
            Assert.NotNull(actualNewAssignmentResult.Value);
        }

        [Fact]
        public async Task TestUpdateAssignmentFailsWith404WhenAssignmentDoesNotExist()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var now = DateTimeOffset.Now;
            var nonExistantAssignmentId = "ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new ProviderWorker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };

            var updateAssignmentVM =
                new UpdateAssignmentBindingModel
                {
                    Status = AssignmentStatus.InProgress,
                    StatusReason = AssignmentStatusReason.WorkerEnRoute,
                    UpdateDateTime = now,
                    InitialLocation = new GeoLocation { Latitude = -34.605062, Longitude = -58.375979 },
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            _testDataStore.EnsureAssignmentDoesNotExist(nonExistantAssignmentId);
            _testDataStore.EnsureClient(client);
            _testDataStore.EnsureProvider(provider);
            _testDataStore.EnsureWorker(worker);

            // act
            var actualResult = await assignmentController.UpdateAssignment(nonExistantAssignmentId, updateAssignmentVM);

            // assert
            var actualUpdateAssignmentResult = actualResult as NotFoundResult;
            Assert.True(actualUpdateAssignmentResult != null, "actualUpdateAssignmentResult should be of type NotFoundObjectResult");
            Assert.Equal(404, actualUpdateAssignmentResult.StatusCode);
        }

        [Fact]
        public async Task TestUpdateAssignmentFailsWith400OnMissingProvider()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var nonExistantProviderId = "ffff0000ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new ProviderWorker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignment =
                new Assignment
                {
                    Id = Guid.NewGuid().ToString("n"),
                    Status = AssignmentStatus.Created,
                    StatusReason = AssignmentStatusReason.New,
                    CreationDateTime = DateTimeOffset.Now,
                    InitialLocation = new GeoJson2DGeographicCoordinates(-58.375979, -34.605062),
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            var updateAssignmentVM =
                new UpdateAssignmentBindingModel
                {
                    Status = AssignmentStatus.InProgress,
                    StatusReason = AssignmentStatusReason.WorkerEnRoute,
                    UpdateDateTime = DateTimeOffset.Now,
                    InitialLocation = new GeoLocation { Latitude = -34.605062, Longitude = -58.375979 },
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = nonExistantProviderId
                };

            _testDataStore.EnsureProviderDoesNotExist(nonExistantProviderId);
            _testDataStore.EnsureClient(client);
            _testDataStore.EnsureProvider(provider);
            _testDataStore.EnsureWorker(worker);
            _testDataStore.EnsureAssignment(assignment);

            // act
            var actualResult = await assignmentController.UpdateAssignment(assignment.Id, updateAssignmentVM);

            // assert
            var actualUpdateAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualUpdateAssignmentResult != null, "actualUpdateAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualUpdateAssignmentResult.StatusCode);
            Assert.NotNull(actualUpdateAssignmentResult.Value);
        }

        [Fact]
        public async Task TestUpdateAssignmentFailsWith400OnMissingWorker()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var nonExistantWorkerId = "ffff0000ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new ProviderWorker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignment =
                new Assignment
                {
                    Id = Guid.NewGuid().ToString("n"),
                    Status = AssignmentStatus.Created,
                    StatusReason = AssignmentStatusReason.New,
                    CreationDateTime = DateTimeOffset.Now,
                    InitialLocation = new GeoJson2DGeographicCoordinates(-58.375979, -34.605062),
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            var updateAssignmentVM =
                new UpdateAssignmentBindingModel
                {
                    Status = AssignmentStatus.InProgress,
                    StatusReason = AssignmentStatusReason.WorkerEnRoute,
                    UpdateDateTime = DateTimeOffset.Now,
                    InitialLocation = new GeoLocation { Latitude = -34.605062, Longitude = -58.375979 },
                    ClientId = client.Id,
                    WorkerId = nonExistantWorkerId,
                    ProviderId = provider.Id
                };

            _testDataStore.EnsureWorkerDoesNotExist(nonExistantWorkerId);
            _testDataStore.EnsureClient(client);
            _testDataStore.EnsureProvider(provider);
            _testDataStore.EnsureWorker(worker);
            _testDataStore.EnsureAssignment(assignment);

            // act
            var actualResult = await assignmentController.UpdateAssignment(assignment.Id, updateAssignmentVM);

            // assert
            var actualUpdateAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualUpdateAssignmentResult != null, "actualUpdateAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualUpdateAssignmentResult.StatusCode);
            Assert.NotNull(actualUpdateAssignmentResult.Value);
        }

        [Fact]
        public async Task TestUpdateAssignmentFailsWith400OnMissingClient()
        {
            // arrange
            var assignmentController = GetAssignmentController();

            var nonExistantClientId = "ffff0000ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new ProviderWorker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignment =
                new Assignment
                {
                    Id = Guid.NewGuid().ToString("n"),
                    Status = AssignmentStatus.Created,
                    StatusReason = AssignmentStatusReason.New,
                    CreationDateTime = DateTimeOffset.Now,
                    InitialLocation = new GeoJson2DGeographicCoordinates(-58.375979, -34.605062),
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            var updateAssignmentVM =
                new UpdateAssignmentBindingModel
                {
                    Status = AssignmentStatus.InProgress,
                    StatusReason = AssignmentStatusReason.WorkerEnRoute,
                    UpdateDateTime = DateTimeOffset.Now,
                    InitialLocation = new GeoLocation { Latitude = -34.605062, Longitude = -58.375979 },
                    ClientId = nonExistantClientId,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            _testDataStore.EnsureClientDoesNotExist(nonExistantClientId);
            _testDataStore.EnsureClient(client);
            _testDataStore.EnsureProvider(provider);
            _testDataStore.EnsureWorker(worker);
            _testDataStore.EnsureAssignment(assignment);

            // act
            var actualResult = await assignmentController.UpdateAssignment(assignment.Id, updateAssignmentVM);

            // assert
            var actualUpdateAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualUpdateAssignmentResult != null, "actualUpdateAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualUpdateAssignmentResult.StatusCode);
            Assert.NotNull(actualUpdateAssignmentResult.Value);
        }

        [Fact]
        public async Task TestSearchAssignmentReturnArgumentInRangeInProgress()
        {
            // arrange
            var assignmentController = GetAssignmentController();
            PopulateDataSet1();
            var searchQuery = new SearchAssignmentBindingModel
            {
                Statuses = new List<AssignmentStatus> { AssignmentStatus.InProgress },
                StartDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 15, 30, 0)),
                EndDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 16, 30, 0))
            };

            // act 
            var actualResult = await assignmentController.SearchAssignments(searchQuery);

            // assert
            var actualUpdateAssignmentResult = actualResult as OkObjectResult;
            Assert.True(actualUpdateAssignmentResult != null, "actualUpdateAssignmentResult should be of type OkObjectResult");
            Assert.Equal(200, actualUpdateAssignmentResult.StatusCode);
            Assert.NotNull(actualUpdateAssignmentResult.Value);
            Assert.Equal(2, ((IEnumerable<AssignmentViewModel>)actualUpdateAssignmentResult.Value).Count());
        }

        [Fact]
        public async Task TestSearchAssignmentReturnArgumentInRangeInProgressAssignedCreated()
        {
            // arrange
            var assignmentController = GetAssignmentController();
            PopulateDataSet1();
            var searchQuery = new SearchAssignmentBindingModel
            {
                Statuses = new List<AssignmentStatus> { AssignmentStatus.InProgress, AssignmentStatus.Assigned, AssignmentStatus.Created },
                StartDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 15, 30, 0)),
                EndDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 16, 30, 0))
            };

            // act 
            var actualResult = await assignmentController.SearchAssignments(searchQuery);

            // assert
            var actualUpdateAssignmentResult = actualResult as OkObjectResult;
            Assert.True(actualUpdateAssignmentResult != null, "actualUpdateAssignmentResult should be of type OkObjectResult");
            Assert.Equal(200, actualUpdateAssignmentResult.StatusCode);
            Assert.NotNull(actualUpdateAssignmentResult.Value);
            Assert.Equal(4, ((IEnumerable<AssignmentViewModel>)actualUpdateAssignmentResult.Value).Count());
        }

        private void PopulateDataSet1()
        {
            var client1Id = Guid.NewGuid().ToString("n");
            var client2Id = Guid.NewGuid().ToString("n");
            _testDataStore.EnsureNoClients();
            _testDataStore.EnsureNoAssignments();
            _testDataStore.EnsureClient(new Client { Id = client1Id });
            _testDataStore.EnsureClient(new Client { Id = client2Id });
            _testDataStore.EnsureAssignment(new Assignment { Id = Guid.NewGuid().ToString("n"), ClientId = client1Id, CreationDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 15, 0, 0)), Status = AssignmentStatus.Created });
            _testDataStore.EnsureAssignment(new Assignment { Id = Guid.NewGuid().ToString("n"), ClientId = client1Id, CreationDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 15, 1, 12)), Status = AssignmentStatus.Completed });
            _testDataStore.EnsureAssignment(new Assignment { Id = Guid.NewGuid().ToString("n"), ClientId = client1Id, CreationDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 15, 30, 0)), Status = AssignmentStatus.InProgress });
            _testDataStore.EnsureAssignment(new Assignment { Id = Guid.NewGuid().ToString("n"), ClientId = client1Id, CreationDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 15, 31, 18)), Status = AssignmentStatus.InProgress });
            _testDataStore.EnsureAssignment(new Assignment { Id = Guid.NewGuid().ToString("n"), ClientId = client2Id, CreationDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 15, 33, 6)), Status = AssignmentStatus.InProgress });
            _testDataStore.EnsureAssignment(new Assignment { Id = Guid.NewGuid().ToString("n"), ClientId = client2Id, CreationDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 15, 45, 0)), Status = AssignmentStatus.Assigned });
            _testDataStore.EnsureAssignment(new Assignment { Id = Guid.NewGuid().ToString("n"), ClientId = client2Id, CreationDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 16, 9, 45)), Status = AssignmentStatus.Created });
            _testDataStore.EnsureAssignment(new Assignment { Id = Guid.NewGuid().ToString("n"), ClientId = client2Id, CreationDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 16, 30, 0)), Status = AssignmentStatus.InProgress });
            _testDataStore.EnsureAssignment(new Assignment { Id = Guid.NewGuid().ToString("n"), ClientId = client2Id, CreationDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 17, 0, 0)), Status = AssignmentStatus.Created });
            _testDataStore.EnsureAssignment(new Assignment { Id = Guid.NewGuid().ToString("n"), ClientId = client2Id, CreationDateTime = new DateTimeOffset(new DateTime(2017, 05, 13, 20, 0, 0)), Status = AssignmentStatus.Completed });
        }

        private AssignmentController GetAssignmentController()
        {
            var controller = 
            new AssignmentController(new AssignmentRepository(_mongoDatabase), new ImageBucketRepository(_mongoDatabase), new NotificationServicesMock());
            var urlMock = new Mock<IUrlHelper>();
            
            urlMock.Setup(_ =>
            _.Link(It.Is<string>(routeName => routeName == "GetAssignment"), It.Is<object>(values => true)))
                .Returns((string routeName, object values) => 
                {
                    var valuesDic = values.ToDictionary();
                    return $"some-host/assignment/{valuesDic["id"]}";
                });
            controller.Url = urlMock.Object;
            return controller;
        }

    }
}
