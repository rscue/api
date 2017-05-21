namespace Rscue.Api.BindingModels
{
    using Rscue.Api.Models;
    using System;
    using System.ComponentModel.DataAnnotations;

    public class NewAssignmentBindingModel
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ProviderId { get; set; }
        
        [Required]
        public DateTimeOffset CreationDateTime { get; set; }

        [Required]
        public GeoLocation InitialLocation { get; set; }

        [Required]
        public AssignmentStatus Status { get; set; }

        [Required]
        public AssignmentStatusReason StatusReason { get; set; }

        public string WorkerId { get; set; }

        public string BoatTowId { get; set; }

        public string Comments { get; set; }

        public TimeSpan? EstimatedTimeOfArrival { get; set; }
    }
}