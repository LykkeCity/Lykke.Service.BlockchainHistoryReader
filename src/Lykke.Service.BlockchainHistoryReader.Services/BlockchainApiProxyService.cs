using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Cache;
using Lykke.Common.Log;
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainHistoryReader.Core.Services;
using Lykke.Service.BlockchainSettings.Client;


namespace Lykke.Service.BlockchainHistoryReader.Services
{
    [UsedImplicitly]
    public class BlockchainApiProxyService : IBlockchainApiProxyService
    {
        private readonly OnDemandDataCache<AssetAccuracy> _assetAccuracyCache;
        private readonly ConcurrentDictionary<string, IBlockchainApiClient> _blockchainApiClients;
        private readonly IBlockchainSettingsClient _blockchainSettingsClient;
        private readonly ILogFactory _logFactory;


        public BlockchainApiProxyService(
            IBlockchainSettingsClient blockchainSettingsClient,
            ILogFactory logFactory)
        {
            _assetAccuracyCache = new OnDemandDataCache<AssetAccuracy>();
            _blockchainApiClients = new ConcurrentDictionary<string, IBlockchainApiClient>();
            _blockchainSettingsClient = blockchainSettingsClient;
            _logFactory = logFactory;
        }

        
        public async Task<IEnumerable<HistoricalTransaction>> GetHistoryOfIncomingTransactionsAsync(
            string blockchainType,
            string address,
            string afterHash,
            int take)
        {
            var blockchainSettings = await _blockchainSettingsClient.GetSettingsByTypeAsync(blockchainType);

            if (blockchainSettings != null)
            {
                var blockchainApiClient = _blockchainApiClients.GetOrAdd
                (
                    blockchainSettings.ApiUrl, 
                    (apiUrl, logFactory) => new BlockchainApiClient(logFactory, apiUrl),
                    _logFactory
                );

                return await blockchainApiClient.GetHistoryOfIncomingTransactionsAsync
                (
                    address: address,
                    afterHash: afterHash,
                    take: take,
                    assetAccuracyProvider: assetId => GetAssetAccuracy(blockchainType, blockchainApiClient, assetId)
                );
            }
            else
            {
                return Enumerable.Empty<HistoricalTransaction>();
            }
        }

        private int GetAssetAccuracy(
            string blockchainType,
            IBlockchainApiClient blockchainApiClient,
            string assetId)
        {
            var assetAccuracy = _assetAccuracyCache.GetOrAdd
            (
                $"{blockchainType}-{assetId}",
                key => new AssetAccuracy(blockchainApiClient.GetAssetAsync(assetId).Result.Accuracy),
                TimeSpan.FromMinutes(15)
            );

            return assetAccuracy.Value;
        }
        
        // OnDemandDataCache requires reference type for value
        private class AssetAccuracy
        {
            public AssetAccuracy(
                int value)
            {
                Value = value;
            }
            
            public int Value { get; }
        }
    }
}
