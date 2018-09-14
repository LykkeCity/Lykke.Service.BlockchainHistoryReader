using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;

namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories
{
    public interface IHistorySourceRepository
    {
        Task DeleteIfExistsAsync(
            string address,
            string blockchainType);

        Task<(IEnumerable<HistorySource> HistorySources, string ContinuationToken)> GetAsync(
            DateTime historyUpdatedOnLimit,
            DateTime historyUpdateScheduledOnLimit,
            string continuationToken);
        
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
