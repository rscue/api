namespace Rscue.Api.Extensions
{
    using Microsoft.AspNetCore.Mvc;
    using Rscue.Api.Models;
    using System;

    public static class ControllerExtensions
    {
        public static IActionResult FromRepositoryOutcome(this Controller controller, RepositoryOutcomeAction outcomeAction, object error, object value = null, string location = null)
        {
            switch (outcomeAction.Outcome)
            {
                case RepositoryOutcome.Ok:
                    if (outcomeAction.Action == RepositoryAction.Created)
                    {
                        return new CreatedResult(location, value);
                    }

                    return controller.Ok(value);
                case RepositoryOutcome.ValidationError: return error == null ? (IActionResult)controller.BadRequest() : controller.BadRequest(error);
                case RepositoryOutcome.NotFound: return error == null ? (IActionResult)controller.NotFound() : controller.NotFound(error);
                default: throw new ArgumentOutOfRangeException(nameof(outcomeAction), outcomeAction.Outcome, "Unrecognized RepositoryOutcome value");
            }
        }
    }
}
