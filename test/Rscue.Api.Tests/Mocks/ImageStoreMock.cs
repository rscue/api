namespace Rscue.Api.Tests.Mocks
{
    using Rscue.Api.Services;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class ImageStoreMock : IImageStore
    {
        private readonly Action<string, Stream> _writer = (name, stream) => { };

        public ImageStoreMock(Action<string, Stream> writer = null)
        {
            if (writer != null)
            {
                _writer = writer;
            }
        }

        public Task DownloadImageAsync(string name, Stream targetStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            _writer(name, targetStream);
            return Task.CompletedTask;
        }

        public Task UploadImageAsync(string name, Stream imageStream, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;
    }
}
