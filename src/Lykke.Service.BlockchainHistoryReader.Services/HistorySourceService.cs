using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.BlockchainHistoryReader.AzureRepositories;
using Lykke.Service.BlockchainHistoryReader.Core.Services;


namespace Lykke.Service.BlockchainHistoryReader.Services
{
    [UsedImplicitly]
    public class HistorySourceService : IHistorySourceService
    {
        private readonly IHistorySourceRepository _historySourceRepository;
        private readonly ILog _log;

        
        public HistorySourceService(
            IHistorySourceRepository historySourceRepository,
            ILogFactory logFactory)
        {
            _historySourceRepository = historySourceRepository;
            _log = logFactory.CreateLog(this);
        }

        
        public async Task AddHistorySourceIfNotExistsAsync(
            string blockchainType,
            string address,
            Guid clientId)
        {
            try
            {
                await _historySourceRepository.GetOrCreateAsync
                (
                    address: address,
                    blockchainType: blockchainType,
                    clientId: clientId
                );
                
                _log.Debug($"History source [{GetHistorySourceIdForLog(blockchainType, address)}] has been added.");
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to add history source [{GetHistorySourceIdForLog(blockchainType, address)}].");
            }
        }

        public async Task DeleteHistorySourceIfExistsAsync(
            string blockchainType,
            string address)
        {
            try
            {
                await _historySourceRepository.DeleteIfExistsAsync
                (
                    address: address,
                    blockchainType: blockchainType
                );
                
                _log.Debug($"History source [{GetHistorySourceIdForLog(blockchainType, address)}] has been deleted.");
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to delete history source [{GetHistorySourceIdForLog(blockchainType, address)}].");
            }
            
            
        }

        private static string GetHistorySourceIdForLog(
            string blockchainType,
            string address)
        {
            return $"{nameof(blockchainType)}: {blockchainType}, {nameof(address)}: {address}";
        }
    }
}
