﻿using System;
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
        public double Latitude { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "El campo es requerido")]
        public AssignmentStatus Status { get; set; }

        public DateTimeOffset UpdateDateTime { get; set; }
        
        public string WorkerId { get; set; }

        public string Comments { get; set; }

        public IList<string> ImageUrls { get; set; }

        public TimeSpan? EstimatedTimeOfArrival { get; set; }
    }
}