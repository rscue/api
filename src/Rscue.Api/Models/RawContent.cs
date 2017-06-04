namespace Rscue.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class RawContent
    {
        public string ContentType { get; set; }

        public Stream Content { get; set; }
    }
}
