namespace Rscue.Api.Tests.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Driver;
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
        public async void TestAddAssignmentSucceedsWith201()
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

        private static AssignmentController GetAssignmentController(IMongoDatabase mongoDatabase) =>
            new AssignmentController(new AssignmentRepository(mongoDatabase), new NotificationServicesMock(), new ImageStoreMock());
    }
}
