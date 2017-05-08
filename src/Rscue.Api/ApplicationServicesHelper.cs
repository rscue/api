using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Rscue.Api.Hubs;
using Rscue.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rscue.Api
{
    public static class ApplicationServicesHelper
    {
        public static void ConfigureApplicationServices(IServiceCollection services, Type imageStoreServiceType = null, Type notificationServicesServiceType = null, Type assignmentRepositoryServiceType = null)
        {
            services.AddTransient(typeof(IImageStore), imageStoreServiceType ?? typeof(ImageStore));
            services.AddTransient(typeof(INotificationServices), notificationServicesServiceType ?? typeof(NotificationServices));
            services.AddTransient(typeof(IAssignmentRepository), assignmentRepositoryServiceType ?? typeof(AssignmentRepository));
            services.AddTransient<IUserIdProvider, HubUserIdProvider>();
        }
    }
}
