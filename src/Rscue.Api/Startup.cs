using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rscue.Api.Models;
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
            services.Configure<AzureSettings>(Configuration.GetSection("AzureSettings"));
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

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
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

        }

        private void ConfigureMongoDb()
        {
            BsonSerializer.RegisterSerializer(new EnumSerializer<VehicleType>(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<HullSizeType>(BsonType.String));
            BsonSerializer.RegisterSerializer(new EnumSerializer<AssignmentStatus>(BsonType.String));
        }

    }
}
