using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace Rscue.Api.Hubs
{
    public class WorkersHub : Hub
    {
    }

    public class HubUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HttpRequest request)
        {
            var user = request.Query["user"];
            return user;
        }
    }
}