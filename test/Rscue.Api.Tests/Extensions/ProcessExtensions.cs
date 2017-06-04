using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Rscue.Api.Tests.Extensions
{
    public static class ProcessExtensions
    {
        public static Process ForceWait(this Process process)
        {
            process.WaitForExit();
            while (!process.HasExited)
            {
                Thread.Sleep(20);
            }

            return process;
        }
    }
}
