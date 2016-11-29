using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rscue.Api.Hubs;
using Rscue.Api.Models;
using Rscue.Api.Plumbing;
using Rscue.Api.ViewModels;

namespace Rscue.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IUserIdProvider, HubUserIdProvider>();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });
            services.Configure<AzureSettings>(Configuration.GetSection("AzureSettings"));
            services.Configure<Auth0Settings>(Configuration.GetSection("Auth0Settings"));
            services.AddMvc().AddJsonOptions(jsonOptions =>
            {
                jsonOptions.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            services.AddScoped<IMongoDatabase>(provider =>
            {
                var client = new MongoClient(Configuration.GetValue<string>("MongoDb:Url"));
                var database = client.GetDatabase(Configuration.GetValue<string>("MongoDB:Database"));
                return database;

            });
            ConfigureMongoDb();

            var settings = new JsonSerializerSettings {ContractResolver = new SignalRContractResolver()};
            var serializer = JsonSerializer.Create(settings);
            services.Add(new ServiceDescriptor(typeof(JsonSerializer), provider => serializer, ServiceLifetime.Transient));
            services.AddSignalR();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("AllowAll");
            app.UseStaticFiles();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddDebug();
            }
            else
            {
                app.UseStatusCodePages();
            }

            app.UseMvc();
            app.UseWebSockets();
            app.UseSignalR();
        }

        private void ConfigureMongoDb()
        {
            BsonSerializer.RegisterSerializer(new EnumSerializer<VehicleType>(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<HullSizeType>(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<AssignmentStatus>(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<WorkerStatus>(BsonType.String));
        }

    }
}
