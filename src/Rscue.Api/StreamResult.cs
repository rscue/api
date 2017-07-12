namespace Rscue.Api
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class StreamResult : IActionResult
    {
        private readonly Func<Task<string>> _contentTypeRetriver;
        private readonly Func<Stream, Task> _contentWriter;

        public StreamResult(Func<Task<string>> contentTypeRetriver, Func<Stream, Task> contentWriter)
        {
            _contentTypeRetriver = contentTypeRetriver ?? throw new ArgumentNullException(nameof(contentTypeRetriver));
            _contentWriter = contentWriter ?? throw new ArgumentNullException(nameof(contentWriter));
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            try
            {
                context.HttpContext.Response.ContentType = await _contentTypeRetriver();
                await _contentWriter(context.HttpContext.Response.Body);
            }
            catch (StorageException ex) when (ex.Message == "The specified blob does not exist.")
            {
                context.HttpContext.Response.StatusCode = 404;
            }
        }
    }
}
