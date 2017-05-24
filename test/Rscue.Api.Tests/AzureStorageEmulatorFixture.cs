namespace Rscue.Api.Tests
{
    using Extensions;
    using System;
    using System.Diagnostics;
    using System.IO;

    public class AzureStorageEmulatorFixture : IDisposable
    {
        /*
         * 
         * issues with using hosted server
         * 
         * https://github.com/Microsoft/vso-agent/issues/72
         * 
         */

        private static string storageEmulatorLocation = @"%ProgramFiles(x86)%\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe";
        private ProcessStartInfo _startASE;
        private ProcessStartInfo _stopASE;
        public AzureStorageEmulatorFixture()
        {
            var aseLocation = Environment.ExpandEnvironmentVariables(storageEmulatorLocation);
            if (!File.Exists(aseLocation))
            {
                throw new Exception("AzureStorageEmulator Not found");
            }

            _startASE = new ProcessStartInfo
            {
                FileName = aseLocation,
                Arguments = "start"
            };

            _stopASE = new ProcessStartInfo()
            {
                FileName = aseLocation,
                Arguments = "stop"
            };

            Process.Start(_startASE)
                .ForceWait()
                .Dispose();
        }

        public void Dispose()
        {
            Process
                .Start(_stopASE)
                .ForceWait()
                .Dispose();
        }
    }
}
