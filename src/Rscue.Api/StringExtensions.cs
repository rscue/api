using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rscue.Api
{
    public static class StringExtensions
    {
        public static string IfNotNullOrEmpty(this string value) => !string.IsNullOrEmpty(value) ? value : null;
    }
}
