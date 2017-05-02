namespace Rscue.Api
{
    using Microsoft.AspNetCore.Mvc;
    using Rscue.Api.Models;
    using System;

    public static class ControllerExtensions
    {
        public static IActionResult FromRepositoryOutcome(this Controller controller, RepositoryOutcome outcome, string message, object value = null, string location = null)
        {
            switch (outcome)
            {
                case RepositoryOutcome.RetrieveSuccess: return controller.Ok(value);
                case RepositoryOutcome.Created: return controller.Created(location, value);
                case RepositoryOutcome.Updated: return controller.NoContent();
                case RepositoryOutcome.ValidationError: return controller.BadRequest(message);
                case RepositoryOutcome.NotFound: return controller.NotFound(value);
                default: throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unrecognized RepositoryOutcome value");
            }
        }
    }
}
