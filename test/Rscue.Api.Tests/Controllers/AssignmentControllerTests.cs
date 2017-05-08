namespace Rscue.Api.Tests.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Driver;
    using MongoDB.Driver.GeoJsonObjectModel;
    using Rscue.Api.Controllers;
    using Rscue.Api.Models;
    using Rscue.Api.Services;
    using Rscue.Api.Tests.Mocks;
    using Rscue.Api.ViewModels;
    using System;
    using Xunit;

    public class AssignmentControllerTests
    {
        [Fact]
        public async void TestGetAssignmentSucceedsWith200OnExistentDocument()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new Worker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignment =
                            new Assignment
                            {
                                ClientId = client.Id,
                                WorkerId = worker.Id,
                                ProviderId = provider.Id,
                                Status = AssignmentStatus.Created,
                                CreationDateTime = DateTimeOffset.Now
                            };

            db.Clients().InsertOne(client);
            db.Providers().InsertOne(provider);
            db.Workers().InsertOne(worker);
            db.Assignments().InsertOne(assignment);

            // act
            var actualResult = await assignmentController.GetAssignment(assignment.Id);

            // assert
            var actualAssignmentResult = actualResult as OkObjectResult;
            var actualAssignment = actualAssignmentResult.Value as Assignment;
            Assert.True(actualAssignmentResult != null, "actualAssignmentResult should be of type OkObjectResult");
            Assert.True(actualAssignment != null, "actualAssignment should be of Type Assignment");
            Assert.Equal(200, actualAssignmentResult.StatusCode);
            Assert.Equal(assignment.Id, actualAssignment.Id);
        }

        [Fact]
        public async void TestGetAssignmentFailsWith404OnNonExistentDocument()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var nonExistantId = "ffff0000ffff0000ffff0000";
            db.Assignments().DeleteOne(_ => _.Id == nonExistantId);

            // act
            var actualResult = await assignmentController.GetAssignment(nonExistantId);

            // assert
            var actualAssignmentResult = actualResult as NotFoundResult;
            Assert.True(actualAssignmentResult != null, "actualAssignmentResult should be of type NotFoundResult");
            Assert.Equal(404, actualAssignmentResult.StatusCode);
        }

        [Fact]
        public async void TestNewAssignmentSucceedsWith201()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new Worker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignmentVM =
                            new AssignmentViewModel
                            {
                                Latitude = -34.605062,
                                Longitude = -58.375979,
                                ClientId = client.Id,
                                WorkerId = worker.Id,
                                ProviderId = provider.Id,
                                Status = AssignmentStatus.Created,
                                CreationDateTime = DateTimeOffset.Now
                            };

            db.Clients().InsertOne(client);
            db.Providers().InsertOne(provider);
            db.Workers().InsertOne(worker);

            // act
            var actualResult = await assignmentController.NewAssignment(assignmentVM);

            // assert
            var actualNewAssignmentResult = actualResult as CreatedAtRouteResult;
            Assert.True(actualNewAssignmentResult != null, "actualNewAssignmentResult should be of type CreatedAtRouteResult");
            Assert.Equal(201, actualNewAssignmentResult.StatusCode);
            Assert.Equal("GetAssignment", actualNewAssignmentResult.RouteName);
            Assert.NotNull(actualNewAssignmentResult.RouteValues["id"]);
        }

        [Fact]
        public async void TestNewAssignmentFailsWith400OnMissingProvider()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var nonExistantProviderId = "ffff0000ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var worker = new Worker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignmentVM =
                            new AssignmentViewModel
                            {
                                Latitude = -34.605062,
                                Longitude = -58.375979,
                                ClientId = client.Id,
                                WorkerId = worker.Id,
                                ProviderId = nonExistantProviderId,
                                Status = AssignmentStatus.Created,
                                CreationDateTime = DateTimeOffset.Now
                            };

            db.Providers().DeleteOne(_ => _.Id == nonExistantProviderId);
            db.Clients().InsertOne(client);
            db.Workers().InsertOne(worker);

            // act
            var actualResult = await assignmentController.NewAssignment(assignmentVM);

            // assert
            var actualNewAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualNewAssignmentResult != null, "actualNewAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualNewAssignmentResult.StatusCode);
            Assert.Equal($"El proveedor con id '{nonExistantProviderId}' no existe", actualNewAssignmentResult.Value);
        }

        [Fact]
        public async void TestNewAssignmentFailsWith400OnMissingWorker()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var nonExistantWorkerId = "ffff0000ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var assignmentVM =
                            new AssignmentViewModel
                            {
                                Latitude = -34.605062,
                                Longitude = -58.375979,
                                ClientId = client.Id,
                                WorkerId = nonExistantWorkerId,
                                ProviderId = provider.Id,
                                Status = AssignmentStatus.Created,
                                CreationDateTime = DateTimeOffset.Now
                            };

            db.Workers().DeleteOne(_ => _.Id == nonExistantWorkerId);
            db.Clients().InsertOne(client);
            db.Providers().InsertOne(provider);

            // act
            var actualResult = await assignmentController.NewAssignment(assignmentVM);

            // assert
            var actualNewAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualNewAssignmentResult != null, "actualNewAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualNewAssignmentResult.StatusCode);
            Assert.Equal($"El trabajador con id '{nonExistantWorkerId}' no existe", actualNewAssignmentResult.Value);
        }

        [Fact]
        public async void TestNewAssignmentFailsWith400OnMissingClient()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var nonExistantClientId = "ffff0000ffff0000ffff0000ffff0000";
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new Worker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignmentVM =
                            new AssignmentViewModel
                            {
                                Latitude = -34.605062,
                                Longitude = -58.375979,
                                ClientId = nonExistantClientId,
                                WorkerId = worker.Id,
                                ProviderId = provider.Id,
                                Status = AssignmentStatus.Created,
                                CreationDateTime = DateTimeOffset.Now
                            };

            db.Clients().DeleteOne(_ => _.Id == nonExistantClientId);
            db.Providers().InsertOne(provider);
            db.Workers().InsertOne(worker);

            // act
            var actualResult = await assignmentController.NewAssignment(assignmentVM);

            // assert
            var actualNewAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualNewAssignmentResult != null, "actualNewAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualNewAssignmentResult.StatusCode);
            Assert.Equal($"El cliente con el id '{nonExistantClientId}' no existe", actualNewAssignmentResult.Value);
        }

        [Fact]
        public async void TestUpdateAssignmentSucceedsWith200()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new Worker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignment =
                new Assignment
                {
                    Status = AssignmentStatus.Created,
                    CreationDateTime = DateTimeOffset.Now,
                    Location = new GeoJson2DGeographicCoordinates(-58.375979, -34.605062),
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            var updateAssignmentVM =
                new AssignmentViewModel
                {
                    Status = AssignmentStatus.InProgress,
                    CreationDateTime = assignment.CreationDateTime,
                    UpdateDateTime = DateTimeOffset.Now,
                    Longitude = -58.375979,
                    Latitude = -34.605062,
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            db.Clients().InsertOne(client);
            db.Providers().InsertOne(provider);
            db.Workers().InsertOne(worker);
            db.Assignments().InsertOne(assignment);

            // act
            var actualResult = await assignmentController.UpdateAssignment(assignment.Id, updateAssignmentVM);

            // assert
            var actualNewAssignmentResult = actualResult as OkObjectResult;
            Assert.True(actualNewAssignmentResult != null, "actualNewAssignmentResult should be of type OkObjectResult");
            Assert.Equal(200, actualNewAssignmentResult.StatusCode);
            Assert.NotNull(actualNewAssignmentResult.Value);
        }

        [Fact]
        public async void TestUpdateAssignmentFailsWith404WhenAssignmentDoesNotExist()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var now = DateTimeOffset.Now;
            var nonExistantAssignmentId = "ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new Worker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };

            var updateAssignmentVM =
                new AssignmentViewModel
                {
                    Status = AssignmentStatus.InProgress,
                    CreationDateTime = now.AddMinutes(-15.0d),
                    UpdateDateTime = now,
                    Longitude = -58.375979,
                    Latitude = -34.605062,
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            db.Assignments().DeleteOne(_ => _.Id == nonExistantAssignmentId);
            db.Clients().InsertOne(client);
            db.Providers().InsertOne(provider);
            db.Workers().InsertOne(worker);
            
            // act
            var actualResult = await assignmentController.UpdateAssignment(nonExistantAssignmentId, updateAssignmentVM);

            // assert
            var actualUpdateAssignmentResult = actualResult as NotFoundResult;
            Assert.True(actualUpdateAssignmentResult != null, "actualUpdateAssignmentResult should be of type NotFoundObjectResult");
            Assert.Equal(404, actualUpdateAssignmentResult.StatusCode);
        }

        [Fact]
        public async void TestUpdateAssignmentFailsWith400OnMissingProvider()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var nonExistantProviderId = "ffff0000ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new Worker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignment =
                new Assignment
                {
                    Status = AssignmentStatus.Created,
                    CreationDateTime = DateTimeOffset.Now,
                    Location = new GeoJson2DGeographicCoordinates(-58.375979, -34.605062),
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            var updateAssignmentVM =
                new AssignmentViewModel
                {
                    Status = AssignmentStatus.InProgress,
                    CreationDateTime = assignment.CreationDateTime,
                    UpdateDateTime = DateTimeOffset.Now,
                    Longitude = -58.375979,
                    Latitude = -34.605062,
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = nonExistantProviderId
                };

            db.Providers().DeleteOne(_ => _.Id == nonExistantProviderId);
            db.Clients().InsertOne(client);
            db.Providers().InsertOne(provider);
            db.Workers().InsertOne(worker);
            db.Assignments().InsertOne(assignment);

            // act
            var actualResult = await assignmentController.UpdateAssignment(assignment.Id, updateAssignmentVM);

            // assert
            var actualUpdateAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualUpdateAssignmentResult != null, "actualUpdateAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualUpdateAssignmentResult.StatusCode);
            Assert.NotNull(actualUpdateAssignmentResult.Value);
        }

        [Fact]
        public async void TestUpdateAssignmentFailsWith400OnMissingWorker()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var nonExistantWorkerId = "ffff0000ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new Worker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignment =
                new Assignment
                {
                    Status = AssignmentStatus.Created,
                    CreationDateTime = DateTimeOffset.Now,
                    Location = new GeoJson2DGeographicCoordinates(-58.375979, -34.605062),
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            var updateAssignmentVM =
                new AssignmentViewModel
                {
                    Status = AssignmentStatus.InProgress,
                    CreationDateTime = assignment.CreationDateTime,
                    UpdateDateTime = DateTimeOffset.Now,
                    Longitude = -58.375979,
                    Latitude = -34.605062,
                    ClientId = client.Id,
                    WorkerId = nonExistantWorkerId,
                    ProviderId = provider.Id
                };

            db.Workers().DeleteOne(_ => _.Id == nonExistantWorkerId);
            db.Clients().InsertOne(client);
            db.Providers().InsertOne(provider);
            db.Workers().InsertOne(worker);
            db.Assignments().InsertOne(assignment);

            // act
            var actualResult = await assignmentController.UpdateAssignment(assignment.Id, updateAssignmentVM);

            // assert
            var actualUpdateAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualUpdateAssignmentResult != null, "actualUpdateAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualUpdateAssignmentResult.StatusCode);
            Assert.NotNull(actualUpdateAssignmentResult.Value);
        }

        [Fact]
        public async void TestUpdateAssignmentFailsWith400OnMissingClient()
        {
            // arrange
            var db = MongoDbHelper.GetRscueCenterUnitTestDatabase();
            var assignmentController = GetAssignmentController(db);

            var nonExistantClientId = "ffff0000ffff0000ffff0000ffff0000";
            var client = new Client { Id = Guid.NewGuid().ToString("n"), Name = "John", LastName = "Carmack" };
            var provider = new Provider { Id = Guid.NewGuid().ToString("n"), Name = "bTow" };
            var worker = new Worker { Id = Guid.NewGuid().ToString("n"), Name = "Andrew", LastName = "Garfield" };
            var assignment =
                new Assignment
                {
                    Status = AssignmentStatus.Created,
                    CreationDateTime = DateTimeOffset.Now,
                    Location = new GeoJson2DGeographicCoordinates(-58.375979, -34.605062),
                    ClientId = client.Id,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            var updateAssignmentVM =
                new AssignmentViewModel
                {
                    Status = AssignmentStatus.InProgress,
                    CreationDateTime = assignment.CreationDateTime,
                    UpdateDateTime = DateTimeOffset.Now,
                    Longitude = -58.375979,
                    Latitude = -34.605062,
                    ClientId = nonExistantClientId,
                    WorkerId = worker.Id,
                    ProviderId = provider.Id
                };

            db.Clients().DeleteOne(_ => _.Id == nonExistantClientId);
            db.Clients().InsertOne(client);
            db.Providers().InsertOne(provider);
            db.Workers().InsertOne(worker);
            db.Assignments().InsertOne(assignment);

            // act
            var actualResult = await assignmentController.UpdateAssignment(assignment.Id, updateAssignmentVM);

            // assert
            var actualUpdateAssignmentResult = actualResult as BadRequestObjectResult;
            Assert.True(actualUpdateAssignmentResult != null, "actualUpdateAssignmentResult should be of type BadRequestObjectResult");
            Assert.Equal(400, actualUpdateAssignmentResult.StatusCode);
            Assert.NotNull(actualUpdateAssignmentResult.Value);
        }

        private static AssignmentController GetAssignmentController(IMongoDatabase mongoDatabase) =>
            new AssignmentController(new AssignmentRepository(mongoDatabase), new NotificationServicesMock(), new ImageStoreMock());
    }
}
