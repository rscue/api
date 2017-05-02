namespace Rscue.Api.Services
{
    using Microsoft.Extensions.Options;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Rscue.Api.Plumbing;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class ImageStore : IImageStore
    {
        private readonly IOptions<AzureSettings> _azureSettings;

        public ImageStore(IOptions<AzureSettings> azureSettings)
        {
            _azureSettings = azureSettings ?? throw new ArgumentNullException(nameof(azureSettings));
        }

        public async Task UploadImageAsync(string name, Stream imageStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            var blockBlob = GetBlockBlobReference(name);
            await blockBlob.UploadFromStreamAsync(imageStream, null, null, null, cancellationToken);
        }

        public async Task DownloadImageAsync(string name, Stream targetStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            var blockBlob = GetBlockBlobReference(name);
            await blockBlob.DownloadToStreamAsync(targetStream, null, null, null, cancellationToken);
        }

        private CloudBlockBlob GetBlockBlobReference(string name)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(_azureSettings.Value.StorageConnectionString);
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference("incidentpics");
            return blobContainer.GetBlockBlobReference(name);
        }
    }
}
