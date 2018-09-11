using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BlockchainHistoryReader.AzureRepositories;
using Lykke.Service.BlockchainHistoryReader.Core.Services;


namespace Lykke.Service.BlockchainHistoryReader.Services
{
    [UsedImplicitly]
    public class HistorySourceService : IHistorySourceService
    {
        private readonly IHistorySourceRepository _historySourceRepository;

        
        public HistorySourceService(
            IHistorySourceRepository historySourceRepository)
        {
            _historySourceRepository = historySourceRepository;
        }

        
        public async Task AddHistorySourceIfNotExistsAsync(
            string blockchainType,
            string address)
        {
            await _historySourceRepository.GetOrCreateAsync
            (
                address: address,
                blockchainType: blockchainType
            );
        }

        public async Task DeleteHistorySourceIfExistsAsync(
            string blockchainType,
            string address)
        {
            await _historySourceRepository.DeleteIfExistsAsync
            (
                address: address,
                blockchainType: blockchainType
            );
        }
    }
}
