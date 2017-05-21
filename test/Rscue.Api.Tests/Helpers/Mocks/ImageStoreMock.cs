namespace Rscue.Api.Tests.Mocks
{
    using Rscue.Api.Services;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;

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

        public Task DeleteImageAsync(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task DownloadImageAsync(string store, string bucket, string name, Stream targetStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            _writer(store + "/" + bucket + "/" + name, targetStream);
            return Task.CompletedTask;
        }

        public Task<string> GetImageContentTypeAsync(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task ProvisionStores(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task<bool> TestImageAsync(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task UploadImageAsync(string name, Stream imageStream, CancellationToken cancellationToken = default(CancellationToken)) => Task.CompletedTask;

        public Task UploadImageAsync(string store, string bucket, string name, string contentType, Stream imageStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<string>> IImageStore.ProvisionStores(CancellationToken cancellationToken)
        {
            return Task.FromResult((IEnumerable<string>)new string[] { });
        }
    }
}
