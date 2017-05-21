namespace Rscue.Api.ViewModels
{
    using Rscue.Api.Models;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class UpdateAssignmentBindingModel
    {
        public string Id { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ProviderId { get; set; }
        
        public GeoLocation InitialLocation { get; set; }

        public GeoLocation ServiceLocation { get; set; }

        [Required]
        public AssignmentStatus Status { get; set; }

        [Required]
        public AssignmentStatusReason StatusReason { get; set; }

        public DateTimeOffset UpdateDateTime { get; set; }
        
        public string WorkerId { get; set; }

        public string BoatTowId { get; set; }

        public string Comments { get; set; }

        public TimeSpan? EstimatedTimeOfArrival { get; set; }
    }
}