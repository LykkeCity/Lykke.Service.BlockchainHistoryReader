using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Service.BlockchainHistoryReader.AzureRepositories.Entities;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;


namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories.Implementations
{
    [UsedImplicitly]
    public class HistorySourceRepository : RepositoryBase, IHistorySourceRepository
    {
        
        // ReSharper disable once NotAccessedField.Local : Reserved for future use
        private readonly IChaosKitty _chaosKitty;
        private readonly INoSQLTableStorage<HistorySourceEntity> _historySources;

        
        private HistorySourceRepository(
            IChaosKitty chaosKitty,
            INoSQLTableStorage<HistorySourceEntity> historySources)
        {
            _chaosKitty = chaosKitty;
            _historySources = historySources;
        }


        public static IHistorySourceRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory,
            IChaosKitty chaosKitty)
        {
            var historySources = AzureTableStorage<HistorySourceEntity>.Create
            (
                connectionString,
                "TransactionHistorySources",
                logFactory
            );
            
            return new HistorySourceRepository(chaosKitty, historySources);
        }

        
        public Task DeleteIfExistsAsync(
            string address,
            string blockchainType)
        {
            return _historySources.DeleteIfExistAsync
            (
                partitionKey: GetPartitionKey(blockchainType),
                rowKey: GetRowKey(address)
            );
        }

        public async Task<HistorySource[]> GetAsync(
            DateTime historyUpdatedOnLimit,
            DateTime historyUpdateScheduledOnLimit)
        {
            string continuationToken = null;
            var entities = new List<HistorySourceEntity>();

            var query = new TableQuery<HistorySourceEntity>()
                .Where
                (
                    TableQuery.GenerateFilterConditionForDate
                    (
                        nameof(HistorySourceEntity.HistoryUpdatedOn),
                        QueryComparisons.LessThanOrEqual,
                        historyUpdatedOnLimit.Date
                    )
                );
            
            do
            {
                IEnumerable<HistorySourceEntity> entitiesBatch;

                (entitiesBatch, continuationToken) = await _historySources.GetDataWithContinuationTokenAsync
                (
                    query,
                    500,
                    continuationToken
                );
                
                entities.AddRange
                (
                    entitiesBatch
                        .Where(x => x.HistoryUpdateScheduledOn <= historyUpdateScheduledOnLimit ||
                                    x.HistoryUpdateScheduledOn >= x.HistoryUpdatedOn)
                );
                
                _chaosKitty.Meow($"{nameof(HistorySourceRepository)}-{nameof(GetAsync)}");

            } while (continuationToken != null);


            return entities
                .Select(RestoreHistorySourceFromEntity)
                .ToArray();
        }

        public async Task<HistorySource> GetOrCreateAsync(
            string address,
            string blockchainType)
        {
            var entity = await _historySources.GetOrInsertAsync
            (
                partitionKey: GetPartitionKey(blockchainType),
                rowKey: GetRowKey(address),
                createNew: () =>
                {
                    var historySource = HistorySource.Create
                    (
                        address: address,
                        blockchainType: blockchainType
                    );

                    return ConvertHistorySourceToEntity(historySource);
                }
            );

            return RestoreHistorySourceFromEntity(entity);
        }

        public Task UpdateAsync(
            HistorySource historySource)
        {
            return _historySources.MergeAsync
            (
                partitionKey: GetPartitionKey(historySource.BlockchainType),
                rowKey: GetRowKey(historySource.Address),
                mergeAction: (entity) =>
                {
                    entity.HistoryUpdatedOn = historySource.HistoryUpdatedOn;
                    entity.HistoryUpdateScheduledOn = historySource.HistoryUpdateScheduledOn;
                    entity.LatestHash = historySource.LatestHash;

                    return entity;
                }
            );
        }

        public async Task<HistorySource> TryGetAsync(
            string address,
            string blockchainType)
        {
            var entity = await _historySources.GetDataAsync
            (
                partition: GetPartitionKey(blockchainType),
                row: GetRowKey(address)
            );

            if (entity != null)
            {
                return HistorySource.Restore
                (
                    address: entity.Address,
                    blockchainType: entity.BlockchainType,
                    historyUpdatedOn: entity.HistoryUpdatedOn,
                    historyUpdateScheduledOn: entity.HistoryUpdateScheduledOn,
                    latestHash: entity.LatestHash
                );
            }
            else
            {
                return null;
            }
        }

        private static string GetPartitionKey(
            string blockchainType)
        {
            return $"{blockchainType}-{blockchainType.CalculateHexHash32(3)}";
        }
        
        private static string GetRowKey(
            string address)
        {
            return address;
        }

        private static HistorySourceEntity ConvertHistorySourceToEntity(
            HistorySource historySource)
        {
            return new HistorySourceEntity
            {
                Address = historySource.Address,
                BlockchainType = historySource.BlockchainType,
                HistoryUpdatedOn = historySource.HistoryUpdatedOn,
                HistoryUpdateScheduledOn = historySource.HistoryUpdateScheduledOn,
                LatestHash = historySource.LatestHash,
                
                PartitionKey = GetPartitionKey(historySource.BlockchainType),
                RowKey = GetRowKey(historySource.Address)
            };
        }

        private static HistorySource RestoreHistorySourceFromEntity(
            HistorySourceEntity entity)
        {
            return HistorySource.Restore
            (
                address: entity.Address,
                blockchainType: entity.BlockchainType,
                historyUpdatedOn: entity.HistoryUpdatedOn,
                historyUpdateScheduledOn: entity.HistoryUpdateScheduledOn,
                latestHash: entity.LatestHash
            );
        }
    }
}
