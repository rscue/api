namespace Rscue.Api.Tests.Services
{
    using Microsoft.Extensions.Options;
    using Rscue.Api.Plumbing;
    using Rscue.Api.Services;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("ImageStore")]
    [Trait("DependsOn", "azure blob storage")]
    public class ImageStoreTests
    {
        private readonly ImageStore _imageStore;

        public ImageStoreTests()
        {
            var storageConnectionString = Environment.GetEnvironmentVariable("RSCUE_API_AzureSettings__StorageConnectionString");
            var azureSettings = new AzureSettings { StorageConnectionString = storageConnectionString };
            _imageStore = new ImageStore(Options.Create(azureSettings));
        }

        [Fact]
        public async Task TestProvisionStoresCompletedWithoutErrors()
        {
            // arrange
            // nothing to do

            // act
            var provisionedStores = await _imageStore.ProvisionStores(CancellationToken.None);

            // assert
            Assert.NotNull(provisionedStores);
            Assert.Collection(
                provisionedStores,
                _ => Assert.Equal(Constants.ASSIGNMENT_IMAGES_STORE, _),
                _ => Assert.Equal(Constants.CLIENT_IMAGES_STORE, _),
                _ => Assert.Equal(Constants.PROVIDER_IMAGES_STORE, _),
                _ => Assert.Equal(Constants.WORKER_IMAGES_STORE, _));

        }

        [Fact]
        public async Task TestUploadImageAndDownloadimage()
        {
            // arrange
            var store = Constants.ASSIGNMENT_IMAGES_STORE;
            var bucket = @"some-bucket";
            var name = Guid.NewGuid().ToString("n");
            var contentType = @"text/plain";
            Stream content = null;
            Stream outputStream = new MemoryStream();
            try
            {
                // arrange cont'd
                content = new MemoryStream(new byte[] { (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o' });
                
                // act
                await _imageStore.UploadImageAsync(store, bucket, name, contentType, content);
                var contentTypeFromStore = await _imageStore.GetImageContentTypeAsync(store, bucket, name);
                await _imageStore.DownloadImageAsync(store, bucket, name, outputStream);
                var imageExists = await _imageStore.TestImageAsync(store, bucket, name);

                // assert
                Assert.True(imageExists);
                Assert.Equal("text/plain", contentTypeFromStore);
                outputStream.Position = 0;
                using (var reader = new StreamReader(outputStream))
                {
                    Assert.Equal("hello", reader.ReadToEnd());
                }
            }
            finally
            {
                // cleanup
                await _imageStore.DeleteImageAsync(store, bucket, name);
                content?.Dispose();
                var imageExists = await _imageStore.TestImageAsync(store, bucket, name);
                Assert.False(imageExists);
            }
        }
    }
}
