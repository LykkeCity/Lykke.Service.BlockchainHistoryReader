using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories.Implementations
{
    [UsedImplicitly]
    public class HistorySourceLockRepository : RepositoryBase, IHistorySourceLockRepository
    {
        private static readonly TimeSpan LockDuration = TimeSpan.FromSeconds(600); 
        
        private readonly CloudBlobContainer _container;
        private readonly string _key;


        private CloudBlockBlob _lockBlob;


        private HistorySourceLockRepository(
            string connectionString,
            string container,
            string key)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            
            _container = blobClient.GetContainerReference(container);
            _key = key;
        }
        
        
        public static IHistorySourceLockRepository Create(
            IReloadingManager<string> connectionString)
        {
            return new HistorySourceLockReloadingDecorator
            (
                async reloadSettings =>
                {
                    var lockRepository = new HistorySourceLockRepository
                    (
                        reloadSettings ? await connectionString.Reload() : connectionString.CurrentValue,
                        "transaction-history-sources",
                        ".lock"
                    );

                    await lockRepository.InitializeAsync();

                    return lockRepository;
                });
        }
        
        private async Task InitializeAsync()
        {
            await _container.CreateIfNotExistsAsync();

            _lockBlob = _container.GetBlockBlobReference(_key);

            if (!await _lockBlob.ExistsAsync())
            {
                await _lockBlob.UploadTextAsync(string.Empty);
            }
        }
        
        public async Task<IHistorySourceLockToken> TryLockAsync()
        {
            try
            {
                var leaseId = await _lockBlob.AcquireLeaseAsync(LockDuration);

                return new LockToken(_lockBlob, leaseId);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == StatusCodes.Status409Conflict)
            {
                return null;
            }
        }
        
        private class LockToken : IHistorySourceLockToken
        {
            private readonly string _leaseId;
            private readonly CloudBlockBlob _lockBlob;
            
            
            public LockToken(
                CloudBlockBlob lockBlob,
                string leaseId)
            {
                _leaseId = leaseId;
                _lockBlob = lockBlob;
            }
            
            
            public Task ReleaseAsync()
            {
                return _lockBlob.ReleaseLeaseAsync(new AccessCondition
                {
                    LeaseId = _leaseId
                });
            }
        }
    }
}
