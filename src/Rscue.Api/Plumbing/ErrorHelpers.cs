using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Rscue.Api.ViewModels;

namespace Rscue.Api.Plumbing
{
    public static class ErrorHelpers
    {
        /// <summary>
        /// Get model state errors
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public static IEnumerable<ErrorViewModel> GetErrors(this ModelStateDictionary modelState)
        {
            var errors = new List<ErrorViewModel>();
            foreach (var keyValuePair in modelState)
            {
                var description = new StringBuilder();
                foreach (var error in keyValuePair.Value.Errors.Take(keyValuePair.Value.Errors.Count - 1))
                {
                    description.AppendLine(error.ErrorMessage);
                }
                description.Append(keyValuePair.Value.Errors.Last().ErrorMessage);
                errors.Add(new ErrorViewModel
                {
                    Field = keyValuePair.Key,
                    Description = description.ToString()
                });
            }
            return errors;
        }
    }
}