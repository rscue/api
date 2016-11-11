using System;
using System.Net;
using System.Threading.Tasks;
using Rscue.Api.Models;
using Rscue.Api.Tests.Helpers;
using Rscue.Api.ViewModels;
using Xunit;

namespace Rscue.Api.Tests
{
    public class AssignmentFixture
    {
        private readonly Random rnd;

        public AssignmentFixture()
        {
            rnd = new Random();
        }

        [Fact]
        public async Task CanAddAssignment()
        {
            var assignment = new AssignmentViewModel
            {
                Latitude = -34.605062,
                Longitude = -58.375979,
                CreationDateTime = DateTimeOffset.Now,
                Status = AssignmentStatus.Created
            };
            var client = new Client
            {
                BoatModel = "ho7",
                Email = "i@h.com",
                EngineType = "hoho hoho",
                HullSize = HullSizeType.Large,
                LastName = "glinsek",
                Name = "nacho",
                PhoneNumber = "123488",
                VehicleType = VehicleType.MotorBoat,
                Id = rnd.Next().ToString()
            };
            var provider = new Provider
            {
                Id = rnd.Next().ToString(),
                Address = "address",
                City = "hoho",
                Email = "ig@ho.com",
                Name = "nonono",
                State = "hohoho",
                ZipCode = "98989"
            };

            await OwinTester.Run(new Post<Client, Client>
            {
                Uri = () => "client",
                Content = () => client,
                ExpectedStatusCode = HttpStatusCode.Created,
                AssertResponseContent = (response, content) =>
                {
                    assignment.ClientId = content.Id;
                }

            }, new Post<Provider, Provider>
            {
                Uri = () => "provider",
                Content = () => provider,
                ExpectedStatusCode = HttpStatusCode.Created,
                AssertResponseContent = (response, content) =>
                {
                    assignment.ProviderId = content.Id;
                }

            }, new Post<AssignmentViewModel, AssignmentViewModel>
            {
                Uri = () => "assignment",
                Content = () => assignment,
                ExpectedStatusCode = HttpStatusCode.Created,
                AssertResponseContent = (response, content) =>
                {
                    Assert.Equal($"http://localhost/assignment/{content.Id}", response.Headers.Location.ToString());                    
                }
            });
        }
    }
}