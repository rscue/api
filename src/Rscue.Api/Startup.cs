using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
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
using Swashbuckle.AspNetCore.Swagger;

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
            services.Configure<ProviderAppSettings>(Configuration.GetSection("ProviderApp"));
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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Rscue center API", Version = "v1" });
                c.DescribeAllEnumsAsStrings();
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                c.DocumentFilter<SecurityRequirementsOperationFilter>();
                c.SchemaFilter<AutoRestSchemaFilter>();
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var secret = Encoding.UTF8.GetBytes(Configuration["Auth0Settings:Secret"]);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                loggerFactory.AddConsole().AddDebug();
            }
            else
            {
                app.UseStatusCodePages();
            }
            app.UseCors("AllowAll");
            app.UseStaticFiles();            
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                TokenValidationParameters =
                {
                    ValidIssuer = $"https://{Configuration["Auth0Settings:Domain"]}/",
                    ValidAudience = Configuration["Auth0Settings:ClientId"],
                    IssuerSigningKey = new SymmetricSecurityKey(secret)
                }
            });
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "docs/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "docs";
                c.SwaggerEndpoint("/docs/v1/swagger.json", "Rscue center API V1");
                c.EnabledValidator();
            });
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
