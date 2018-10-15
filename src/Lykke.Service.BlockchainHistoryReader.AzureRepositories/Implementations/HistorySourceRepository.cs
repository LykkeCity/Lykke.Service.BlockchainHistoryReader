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
    public class HistorySourceRepository : IHistorySourceRepository
    {
        
        private readonly INoSQLTableStorage<HistorySourceEntity> _historySources;

        
        private HistorySourceRepository(
            INoSQLTableStorage<HistorySourceEntity> historySources)
        {
            _historySources = historySources;
        }


        public static IHistorySourceRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            var historySources = AzureTableStorage<HistorySourceEntity>.Create
            (
                connectionString,
                "TransactionHistorySources",
                logFactory
            );
            
            return new HistorySourceRepository(historySources);
        }

        
        public Task DeleteIfExistsAsync(
            string address,
            string blockchainType)
        {
            return _historySources.DeleteIfExistAsync
            (
                partitionKey: GetPartitionKey(blockchainType, address),
                rowKey: GetRowKey(address)
            );
        }

        public async Task<(IEnumerable<HistorySource> HistorySources, string ContinuationToken)> GetAsync(
            DateTime historyUpdatedOnLimit,
            DateTime historyUpdateScheduledOnLimit,
            string continuationToken)
        {
            var query = new TableQuery<HistorySourceEntity>()
                .Where
                (
                    TableQuery.GenerateFilterConditionForDate
                    (
                        nameof(HistorySourceEntity.HistoryUpdatedOn),
                        QueryComparisons.LessThanOrEqual,
                        historyUpdatedOnLimit
                    )
                );
            
            IEnumerable<HistorySourceEntity> entities;

            (entities, continuationToken) = await _historySources.GetDataWithContinuationTokenAsync
            (
                query,
                500,
                continuationToken
            );

            var historySources = entities.Where(x => x.HistoryUpdateScheduledOn <= historyUpdateScheduledOnLimit ||
                                                     x.HistoryUpdateScheduledOn <= x.HistoryUpdatedOn)
                                         .Select(RestoreHistorySourceFromEntity);


            return (historySources, continuationToken);
        }
        
        public async Task<HistorySource> GetOrCreateAsync(
            string address,
            string blockchainType,
            Guid clientId)
        {
            var entity = await _historySources.GetOrInsertAsync
            (
                partitionKey: GetPartitionKey(blockchainType, address),
                rowKey: GetRowKey(address),
                createNew: () =>
                {
                    var historySource = HistorySource.Create
                    (
                        address: address,
                        blockchainType: blockchainType,
                        clientId: clientId
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
                partitionKey: GetPartitionKey(historySource.BlockchainType, historySource.Address),
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
                partition: GetPartitionKey(blockchainType, address),
                row: GetRowKey(address)
            );

            if (entity != null)
            {
                return HistorySource.Restore
                (
                    address: entity.Address,
                    blockchainType: entity.BlockchainType,
                    clientId: entity.ClientId,
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
            string blockchainType,
            string address)
        {
            return $"{blockchainType}-{address.CalculateHexHash32(3)}";
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
                ClientId = historySource.ClientId,
                HistoryUpdatedOn = historySource.HistoryUpdatedOn,
                HistoryUpdateScheduledOn = historySource.HistoryUpdateScheduledOn,
                LatestHash = historySource.LatestHash,
                
                PartitionKey = GetPartitionKey(historySource.BlockchainType, historySource.Address),
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
                clientId: entity.ClientId,
                historyUpdatedOn: entity.HistoryUpdatedOn,
                historyUpdateScheduledOn: entity.HistoryUpdateScheduledOn,
                latestHash: entity.LatestHash
            );
        }
    }
}
