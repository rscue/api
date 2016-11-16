using System;
using System.Collections.Generic;
using Rscue.Api.Models;

namespace Rscue.Api.ViewModels
{
    public class AssignmentSearchViewModel
    {
        public IEnumerable<AssignmentStatus> Statuses { get; set; }

        public DateTimeOffset? StartDateTime { get; set; }

        public DateTimeOffset? EndDateTime { get; set; }
    }
}