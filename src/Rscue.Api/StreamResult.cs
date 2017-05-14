using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rscue.Api
{
    public class StreamResult : IActionResult
    {
        private Func<Stream, Task> _streamWriter;
        public StreamResult(Func<Stream, Task> streamWriter)
        {
            _streamWriter = streamWriter;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            await _streamWriter(context.HttpContext.Response.Body);
        }
    }
}
