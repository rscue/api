namespace Rscue.Api.Tests
{
    using Extensions;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    public class AzureStorageEmulatorFixture : IDisposable
    {
        private Process _process;
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
                Arguments = "start -inprocess"
            };

            _stopASE = new ProcessStartInfo()
            {
                FileName = aseLocation,
                Arguments = "stop"
            };

            _process = Process.Start(_startASE);
        }

        public void Dispose()
        {
            Process
                .Start(_stopASE)
                .ForceWait()
                .Dispose();

            _process
                .ForceWait()
                .Dispose();
        }
    }
}
