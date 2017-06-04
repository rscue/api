namespace Rscue.Api.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Rscue.Api.Models;

    public class SearchAssignmentBindingModel
    {
        public IEnumerable<AssignmentStatus> Statuses { get; set; }

        public DateTimeOffset? StartDateTime { get; set; }

        public DateTimeOffset? EndDateTime { get; set; }
    }
}