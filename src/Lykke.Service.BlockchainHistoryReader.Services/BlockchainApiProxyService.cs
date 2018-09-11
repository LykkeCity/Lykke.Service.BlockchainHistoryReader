using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
        private readonly ConcurrentDictionary<string, IBlockchainApiClient> _blockchainApiClients;
        private readonly IBlockchainSettingsClient _blockchainSettingsClient;
        private readonly ILogFactory _logFactory;


        public BlockchainApiProxyService(
            IBlockchainSettingsClient blockchainSettingsClient,
            ILogFactory logFactory)
        {
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
            var blockchainClient = _blockchainApiClients.GetOrAdd
            (
                blockchainSettings.ApiUrl, 
                (apiUrl, logFactory) => new BlockchainApiClient(logFactory, apiUrl),
                _logFactory
            );

            return await blockchainClient.GetHistoryOfIncomingTransactionsAsync
            (
                address: address,
                afterHash: afterHash,
                take: take,
                assetAccuracyProvider: assetId => blockchainClient.GetAssetAsync(assetId).Result.Accuracy
            );
        }
    }
}
