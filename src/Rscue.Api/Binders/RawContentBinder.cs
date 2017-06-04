namespace Rscue.Api.ModelBinders
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Rscue.Api.Models;
    using System.IO;
    using System.Threading.Tasks;

    public class RawContentBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var contentType = (string)null;
            var content = (Stream)null;
            if (bindingContext.ActionContext.HttpContext.Request.HasFormContentType && bindingContext.ActionContext.HttpContext.Request.Form.Files.Count != 0)
            {
                contentType = bindingContext.ActionContext.HttpContext.Request.Form.Files[0].ContentType;
                content = new MemoryStream();
                // investigate if this could be sustituted by OpenReadStream without leaking
                await bindingContext.ActionContext.HttpContext.Request.Form.Files[0].CopyToAsync(content);
                content.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                contentType = bindingContext.ActionContext.HttpContext.Request.ContentType;
                content = bindingContext.ActionContext.HttpContext.Request.Body;
            }

            bindingContext.Result = 
                ModelBindingResult.Success(
                    new RawContent
                    {
                        Content = content,
                        ContentType = contentType
                    });
        }
    }
}
