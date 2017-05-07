namespace Rscue.Api
{
    using Microsoft.AspNetCore.Mvc;
    using Rscue.Api.Models;
    using System;

    public static class ControllerExtensions
    {
        public static IActionResult FromRepositoryOutcome(this Controller controller, RepositoryOutcome outcome, object error, object value = null, string routeName = null, object routeValues = null)
        {
            switch (outcome)
            {
                case RepositoryOutcome.RetrieveSuccess: return controller.Ok(value);
                case RepositoryOutcome.Created: return controller.CreatedAtRoute(routeName, routeValues, value);
                case RepositoryOutcome.Updated: return controller.NoContent();
                case RepositoryOutcome.ValidationError: return error == null ? (IActionResult)controller.BadRequest() : controller.BadRequest(error);
                case RepositoryOutcome.NotFound: return value == null ? (IActionResult)controller.NotFound() : controller.NotFound(value);
                default: throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Unrecognized RepositoryOutcome value");
            }
        }
    }
}
