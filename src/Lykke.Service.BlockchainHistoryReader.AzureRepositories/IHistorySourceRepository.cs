using System;
using System.Threading.Tasks;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;

namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories
{
    public interface IHistorySourceRepository
    {
        Task DeleteIfExistsAsync(
            string address,
            string blockchainType);

        Task<HistorySource[]> GetAsync(
            DateTime historyUpdatedOnLimit,
            DateTime historyUpdateScheduledOnLimit);
        
        Task<HistorySource> GetOrCreateAsync(
            string address,
            string blockchainType);
        
        Task UpdateAsync(
            HistorySource historySource);

        Task<HistorySource> TryGetAsync(
            string address,
            string blockchainType);
    }
}
