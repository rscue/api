namespace Rscue.Api.ModelBinders
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Rscue.Api.Models;
    using System.Threading.Tasks;

    public class RawContentBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            bindingContext.Result = 
                ModelBindingResult.Success(
                    new RawContent
                    {
                        Content = bindingContext.ActionContext.HttpContext.Request.Body,
                        ContentType = bindingContext.ActionContext.HttpContext.Request.ContentType
                    });

            return Task.CompletedTask;
        }
    }
}
