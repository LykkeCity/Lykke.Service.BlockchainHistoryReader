using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Blob;
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
    public class HistorySourceLockRepository : IHistorySourceLockRepository
    {
        private static readonly TimeSpan LockDuration = TimeSpan.FromSeconds(60);

        private readonly IBlobStorage _blobStorage;
        private readonly string _container;
        private readonly ILog _log;
        private readonly string _key;


        private HistorySourceLockRepository(
            IBlobStorage blobStorage,
            string container,
            ILogFactory logFactory,
            string key)
        {
            _blobStorage = blobStorage;
            _container = container;
            _log = logFactory.CreateLog(this);
            _key = key;
        }
        
        
        public static IHistorySourceLockRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            return new HistorySourceLockRepository
            (
                blobStorage: AzureBlobStorage.Create(connectionString),
                container: "transaction-history-sources",
                logFactory: logFactory,
                key: ".lock"
            );
        }
        
        public async Task<IHistorySourceLockToken> TryLockAsync()
        {
            try
            {
                var leaseId = await _blobStorage.AcquireLeaseAsync
                (
                    container: _container, 
                    key: _key,
                    leaseTime: LockDuration
                );

                _log.Debug("History sources have been locked.");
                
                return new LockToken
                (
                    blobStorage: _blobStorage,
                    container: _container,
                    key: _key,
                    leaseId: leaseId,
                    log: _log
                );
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == StatusCodes.Status409Conflict)
            {
                return null;
            }
        }
        
        private class LockToken : IHistorySourceLockToken
        {
            private readonly IBlobStorage _blobStorage;
            private readonly string _container;
            private readonly string _key;
            private readonly string _leaseId;
            private readonly ILog _log;

            private DateTime _renewAfter;

            
            public LockToken(
                IBlobStorage blobStorage,
                string container,
                string key,
                string leaseId,
                ILog log)
            {
                _blobStorage = blobStorage;
                _container = container;
                _key = key;
                _leaseId = leaseId;
                _log = log;
                
                UpdateRenewAfter();
            }
            
            
            public async Task ReleaseAsync()
            {
                try
                {
                    await  _blobStorage.ReleaseLeaseAsync
                    (
                        container: _container,
                        key: _key, 
                        leaseId: _leaseId
                    );
                    
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
                        await _blobStorage.RenewLeaseAsync
                        (
                            container: _container,
                            key: _key,
                            leaseId: _leaseId
                        );
                    
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
