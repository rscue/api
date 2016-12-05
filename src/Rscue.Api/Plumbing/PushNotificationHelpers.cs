using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rscue.Api.Plumbing
{
    public static class PushNotificationHelpers
    {
        public static async Task Send(string applicationId, string senderId, dynamic payload)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://fcm.googleapis.com/fcm/send");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={applicationId}");
                httpClient.DefaultRequestHeaders.Add("Sender", $"id={senderId}");

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await httpClient.PostAsync(string.Empty, content);
            }
        }

        public static dynamic GetNewAssignmentPayload(string deviceId, string assignmentId)
        {
            return new
            {
                to = deviceId,
                priority = "high",
                content_available = true,
                data = new
                {
                    title = "Nueva misión",
                    body = "Hay un nuevo pedido de remolque",
                    sound = "default",
                    assignmentId = assignmentId
                }
            };
        }
    }
}