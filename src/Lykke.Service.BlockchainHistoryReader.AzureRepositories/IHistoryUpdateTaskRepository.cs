using System;
using System.Threading.Tasks;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;

namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories
{
    public interface IHistoryUpdateTaskRepository
    {
        Task CompleteAsync(
            HistoryUpdateTask task);

        Task EnqueueAsync(
            HistoryUpdateTask task);

        Task<HistoryUpdateTask> TryGetAsync(
            TimeSpan visibilityTimeout);
    }
}
