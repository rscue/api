using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace Rscue.Api.Tests.Helpers
{
    public static class OwinTester
    {
        private static readonly HttpClient Client;

        /// <summary>
        /// Configuration variables
        /// </summary>
        public static IConfigurationRoot Configuration { get; }

        static OwinTester()
        {
            var server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>());
            Client = server.CreateClient();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public static async Task Run(params Step[] steps)
        {
            if (steps.Length == 0)
            {
                throw new ArgumentException("One Step at least is required", nameof(steps));
            }

            int stepNumber = 1;

            foreach (var step in steps)
            {
                var f = new Func<HttpResponseMessage, bool>(r => r.IsSuccessStatusCode);
                var expectedStatusCodeMessage = "20x";

                if (string.IsNullOrEmpty(step.BuiltRelativeUri))
                {
                    throw new ArgumentNullException(
                        $"Error in step number {stepNumber}. Step.Uri cannot be null or empty.");
                }

                var request = step.BuildHttpRequestMessage();

                if (step.AuthenticationHeaderValue != null)
                {
                    request.Headers.Authorization = step.AuthenticationHeaderValue();
                }

                request.Headers.Host = request.RequestUri.Host;
                var response = await Client.SendAsync(request);

                if (step.ExpectedStatusCode.HasValue)
                {
                    var step1 = step;
                    f = (r => r.StatusCode == step1.ExpectedStatusCode);
                    expectedStatusCodeMessage = step.ExpectedStatusCode.ToString();
                }

                Assert.True(f(response),
                    $"Unexpected response status code in step number {stepNumber} (Expected: {expectedStatusCodeMessage} / Actual: {response.StatusCode}).");

                step.AssertResponse?.Invoke(response);

                if (step.Type != null && ((dynamic)step).AssertResponseContent != null)
                {
                    var result = response.Content.ReadAsAsync(step.Type).Result;
                    Assert.NotNull(result);

                    var action = ((dynamic)step).AssertResponseContent;
                    ((Delegate)action).DynamicInvoke(response, result);
                }
                stepNumber++;
            }
        }
    }

    public class Post<TContent, TResponse> : Post<TContent>
    {
        internal override Type Type => typeof(TResponse);
        public Action<HttpResponseMessage, TResponse> AssertResponseContent { get; set; }
    }

    public class Post<TContent> : Step
    {
        private readonly Lazy<TContent> _builtContent;
        public Func<TContent> Content { get; set; }

        public Post()
        {
            _builtContent = new Lazy<TContent>(() => Content());
        }

        public override HttpRequestMessage BuildHttpRequestMessage()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(UriBase, BuiltRelativeUri))
            {
                Content = new ObjectContent<TContent>(_builtContent.Value, new JsonMediaTypeFormatter
                {
                    SerializerSettings = new JsonSerializerSettings
                    {
                        DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
                    }
                })
            };

            return request;
        }
    }

    public class Get<TResponse> : Get
    {
        internal override Type Type => typeof(TResponse);

        public Action<HttpResponseMessage, TResponse> AssertResponseContent { get; set; }
    }

    public class Get : Step
    {
        public override HttpRequestMessage BuildHttpRequestMessage()
        {
            return new HttpRequestMessage(HttpMethod.Get, new Uri(UriBase, BuiltRelativeUri));
        }
    }

    public abstract class Step
    {
        protected static readonly Uri UriBase = new Uri("http://localhost");
        private readonly Lazy<string> _builtRelativeUri;

        protected Step()
        {
            _builtRelativeUri = new Lazy<string>(() => Uri());
        }

        internal virtual Type Type => null;

        public Func<string> Uri { get; set; }
        internal string BuiltRelativeUri => _builtRelativeUri.Value;
        public HttpStatusCode? ExpectedStatusCode { get; set; }
        public Action<HttpResponseMessage> AssertResponse { get; set; }

        public Func<AuthenticationHeaderValue> AuthenticationHeaderValue { get; set; }
        public abstract HttpRequestMessage BuildHttpRequestMessage();
    }

    public class Put<TContent, TResponse> : Put<TContent>
    {
        internal override Type Type => typeof(TResponse);
        public Action<HttpResponseMessage, TResponse> AssertResponseContent { get; set; }
    }

    public class Put<TContent> : Step
    {
        private readonly Lazy<TContent> _builtContent;
        public Func<TContent> Content { get; set; }

        public Put()
        {
            _builtContent = new Lazy<TContent>(() => Content());
        }

        public override HttpRequestMessage BuildHttpRequestMessage()
        {
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(UriBase, BuiltRelativeUri))
            {
                Content = new ObjectContent<TContent>(_builtContent.Value, new JsonMediaTypeFormatter
                {
                    SerializerSettings = new JsonSerializerSettings
                    {
                        DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
                    }
                })
            };

            return request;
        }
    }
}