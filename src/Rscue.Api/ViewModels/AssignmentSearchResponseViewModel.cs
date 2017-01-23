using System;
using Rscue.Api.Models;

namespace Rscue.Api.ViewModels
{
    public class AssignmentSearchResponseViewModel
    {
        public string Id { get; set; }
        public string ClientName { get; set; }
        public string WorkerName { get; set; }
        public DateTimeOffset CreationDateTime { get; set; }
        public AssignmentStatus Status { get; set; }
        public TimeSpan? EstimatedTimeOfArrival { get; set; }
    }
}