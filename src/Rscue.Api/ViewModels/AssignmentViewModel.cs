﻿namespace Rscue.Api.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Rscue.Api.Models;

    public class AssignmentViewModel
    {
        public string Id { get; set; }

        public string ClientId { get; set; }

        public string ProviderId { get; set; }
        
        public DateTimeOffset CreationDateTime { get; set; }

        public GeoLocation InitialLocation { get; set; }

        public GeoLocation ServiceLocation { get; set; }

        public AssignmentStatus Status { get; set; }

        public AssignmentStatusReason StatusReason { get; set; }

        public DateTimeOffset? UpdateDateTime { get; set; }
        
        public string WorkerId { get; set; }

        public string BoatTowId { get; set; }

        public string Comments { get; set; }

        public string ImagesUrl { get; set; }

        public string ClientName { get; set; }
        public string WorkerName { get; set; }
        public string ClientAvatarUri { get; set; }
        public TimeSpan? EstimatedTimeOfArrival { get; set; }
    }
}