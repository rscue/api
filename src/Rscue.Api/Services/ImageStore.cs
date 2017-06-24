namespace Rscue.Api.Services
{
    using Microsoft.Extensions.Options;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Rscue.Api.Plumbing;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ImageStore : IImageStore
    {
        private static readonly string[] stores = new string[] { Constants.ASSIGNMENT_IMAGES_STORE, Constants.CLIENT_IMAGES_STORE, Constants.PROVIDER_IMAGES_STORE, Constants.WORKER_IMAGES_STORE };

        private readonly IOptions<AzureSettings> _azureSettings;

        public ImageStore(IOptions<AzureSettings> azureSettings)
        {
            _azureSettings = azureSettings ?? throw new ArgumentNullException(nameof(azureSettings));
        }

        public async Task UploadImageAsync(string store, string bucket, string name, string contentType, Stream imageStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateParameters(store, bucket, name);
            var blockBlob = GetBlockBlobReference(store, GetImageReference(bucket, name));
            await blockBlob.UploadFromStreamAsync(imageStream, null, null, null, cancellationToken);
            blockBlob.Properties.ContentType = contentType;
            await blockBlob.SetPropertiesAsync();
        }

        public async Task DownloadImageAsync(string store, string bucket, string name, Stream targetStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateParameters(store, bucket, name);
            var blockBlob = GetBlockBlobReference(store, GetImageReference(bucket, name));
            await blockBlob.DownloadToStreamAsync(targetStream, null, null, null, cancellationToken);
        }

        public async Task<string> GetImageContentTypeAsync(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateParameters(store, bucket, name);
            var blockBlob = GetBlockBlobReference(store, GetImageReference(bucket, name));
            await blockBlob.FetchAttributesAsync();
            return blockBlob.Properties.ContentType;
        }

        public async Task DeleteImageAsync(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateParameters(store, bucket, name);
            var blockBlob = GetBlockBlobReference(store, GetImageReference(bucket, name));
            await blockBlob.DeleteAsync();
        }

        public async Task<IEnumerable<string>> ProvisionStores(CancellationToken cancellationToken = default(CancellationToken))
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(_azureSettings.Value.StorageConnectionString);
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var results = new List<string>();
            foreach (var store in stores)
            {
                var blockBlobReference = blobClient.GetContainerReference(store);
                var provisioned = await blockBlobReference.CreateIfNotExistsAsync();
                results.Add(store);
            }

            return results;
        }

        public async Task<bool> TestImageAsync(string store, string bucket, string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateParameters(store, bucket, name);
            var blockBlob = GetBlockBlobReference(store, GetImageReference(bucket, name));
            return await blockBlob.ExistsAsync();
        }

        private static void ValidateParameters(string store, string bucket, string name)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (bucket == null) throw new ArgumentNullException(nameof(bucket));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (!stores.Contains(store)) throw new ArgumentOutOfRangeException(nameof(store), store, "Unrecognized store");
        }

        private static string GetImageReference(string bucket, string name) => bucket + "/" + name;

        private CloudBlockBlob GetBlockBlobReference(string store, string reference)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(_azureSettings.Value.StorageConnectionString);
            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(store);
            return blobContainer.GetBlockBlobReference(reference);
        }
    }
}
