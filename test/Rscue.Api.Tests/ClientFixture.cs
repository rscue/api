using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Rscue.Api.Models;
using Rscue.Api.Tests.Helpers;
using Rscue.Api.ViewModels;
using Xunit;

namespace Rscue.Api.Tests
{
    public class ClientFixture
    {
        [Fact]
        public async Task CanSaveClient()
        {
            var client = new ClientViewModel
            {
                BoatModel = "ho7",
                Email = "i@h.com",
                EngineType = "hoho hoho",
                HullSize = HullSizeType.Large,
                LastName = "glinsek",
                Name = "nacho",
                PhoneNumber = "123488",
                VehicleType = VehicleType.MotorBoat
            };

            await OwinTester.Run(new Post<ClientViewModel, ClientViewModel>()
            {
                Uri = () => "client",
                Content = () => client,
                ExpectedStatusCode = HttpStatusCode.Created,
                AssertResponseContent = (response, content) =>
                {
                    Assert.Equal($"http://localhost/client/{content.Id}", response.Headers.Location.ToString());
                }

            });
        }

        [Fact]
        public async Task CanGetClient()
        {
            var client = new ClientViewModel
            {
                BoatModel = "ho7",
                Email = "i@h.com",
                EngineType = "hoho hoho",
                HullSize = HullSizeType.Large,
                LastName = "glinsek",
                Name = "nacho",
                PhoneNumber = "123488",
                VehicleType = VehicleType.MotorBoat
            };
            var id = String.Empty;

            await OwinTester.Run(new Post<ClientViewModel, ClientViewModel>()
            {
                Uri = () => "client",
                Content = () => client,
                ExpectedStatusCode = HttpStatusCode.Created,
                AssertResponseContent = (response, content) =>
                {
                    id = content.Id;
                }

            }, new Get<ClientViewModel>()
            {
                Uri = () => $"client/{id}",
                ExpectedStatusCode = HttpStatusCode.OK,
                AssertResponseContent = (response, content) =>
                {
                    Assert.NotNull(content);
                    Assert.Equal(id, content.Id);
                }
            });
        }
    }
}
