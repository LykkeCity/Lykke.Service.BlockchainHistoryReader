using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories.Implementations
{
    [UsedImplicitly]
    public class HistorySourceLockRepository : RepositoryBase, IHistorySourceLockRepository
    {
        private static readonly TimeSpan LockDuration = TimeSpan.FromSeconds(60); 
        
        private readonly CloudBlobContainer _container;
        private readonly ILog _log;
        private readonly string _key;


        private CloudBlockBlob _lockBlob;


        private HistorySourceLockRepository(
            ILogFactory logFactory,
            string connectionString,
            string container,
            string key)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            
            _container = blobClient.GetContainerReference(container);
            _log = logFactory.CreateLog(this);
            _key = key;
        }
        
        
        public static IHistorySourceLockRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            return new HistorySourceLockReloadingDecorator
            (
                async reloadSettings =>
                {
                    var lockRepository = new HistorySourceLockRepository
                    (
                        logFactory: logFactory,
                        connectionString: reloadSettings ? await connectionString.Reload() : connectionString.CurrentValue,
                        container: "transaction-history-sources",
                        key: ".lock"
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

                _log.Debug("History sources have been locked.");
                
                return new LockToken(_lockBlob, _log, leaseId);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == StatusCodes.Status409Conflict)
            {
                return null;
            }
        }
        
        private class LockToken : IHistorySourceLockToken
        {
            private readonly string _leaseId;
            private readonly ILog _log;
            private readonly CloudBlockBlob _lockBlob;

            private DateTime _renewAfter;

            
            public LockToken(
                CloudBlockBlob lockBlob,
                ILog log,
                string leaseId)
            {
                _leaseId = leaseId;
                _log = log;
                _lockBlob = lockBlob;
                
                UpdateRenewAfter();
            }
            
            
            public async Task ReleaseAsync()
            {
                try
                {
                    await  _lockBlob.ReleaseLeaseAsync(new AccessCondition
                    {
                        LeaseId = _leaseId
                    });
                    
                    _log.Debug("History sources lock has been released.");
                }
                catch (Exception e)
                {
                    _log.Warning("Failed to release history sources lock.", e);
                }
            }

            public async Task RenewIfNecessaryAsync()
            {
                try
                {
                    if (DateTime.UtcNow > _renewAfter)
                    {
                        await _lockBlob.RenewLeaseAsync(new AccessCondition
                        {
                            LeaseId = _leaseId
                        });
                    
                        UpdateRenewAfter();
                        
                        _log.Debug("History source lock has been renewed.");
                    }
                }
                catch (Exception e)
                {
                    _log.Warning("Failed to renew history source lock.", e);
                }
            }

            private void UpdateRenewAfter()
            {
                _renewAfter = DateTime.UtcNow + (LockDuration / 2);
            }
        }
    }
}
