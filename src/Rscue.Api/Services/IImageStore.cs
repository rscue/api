namespace Rscue.Api.Services
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IImageStore
    {
        Task UploadImageAsync(string name, Stream imageStream, CancellationToken cancellationToken = default(CancellationToken));

        Task DownloadImageAsync(string name, Stream targetStream, CancellationToken cancellationToken = default(CancellationToken));
    }
}
