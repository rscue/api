namespace Rscue.Api.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IImageStore
    {
        Task UploadImageAsync(string store, string bucket, string name, string contentType, Stream imageStream, CancellationToken cancellationToken = default(CancellationToken));

        Task DownloadImageAsync(string store, string bucket, string name, Stream targetStream, CancellationToken cancellationToken = default(CancellationToken));

        Task<string> GetImageContentTypeAsync(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteImageAsync(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> TestImageAsync(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken));

        Task<IEnumerable<string>> ProvisionStores(CancellationToken cancellationToken = default(CancellationToken));
    }
}
