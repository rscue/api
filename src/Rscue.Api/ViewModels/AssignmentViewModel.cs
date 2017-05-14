using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Rscue.Api.Models;

namespace Rscue.Api.ViewModels
{
    public class AssignmentViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string ClientId { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public string ProviderId { get; set; }
        
        [Required(ErrorMessage = "El campo es requerido")]
        public DateTimeOffset CreationDateTime { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public double InitialLocationLatitude { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public double InitialLocationLongitude { get; set; }

        public double? ServiceLocationLatitude { get; set; }

        public double? ServiceLocationLongitude { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public AssignmentStatus Status { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public AssignmentStatusReason StatusReason { get; set; }

        public DateTimeOffset UpdateDateTime { get; set; }
        
        public string WorkerId { get; set; }

        public string BoatTowId { get; set; }

        public string Comments { get; set; }

        public IList<string> ImageUrls { get; set; }

        public TimeSpan? EstimatedTimeOfArrival { get; set; }
    }
}