namespace Rscue.Api
{
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;
    using Rscue.Api.Hubs;
    using Rscue.Api.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class ApplicationServicesHelper
    {
        public static void ConfigureApplicationServices(IServiceCollection services)
        {
            services.AddTransient<IImageStore, ImageStore>();
            services.AddTransient<IImageBucketRepository, ImageBucketRepository>();
            services.AddTransient<INotificationServices, NotificationServices>();
            services.AddTransient<IAssignmentRepository, AssignmentRepository>();
            services.AddTransient<IProviderRepository, ProviderRepository>();
            services.AddTransient<IUserIdProvider, HubUserIdProvider>();
        }
    }
}
