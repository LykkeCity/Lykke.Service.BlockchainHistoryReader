using System;
using System.Threading.Tasks;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;

namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories
{
    public interface IHistoryUpdateTaskRepository
    {
        Task CompleteAsync(
            string completionToken);

        Task EnqueueAsync(
            HistoryUpdateTask task);

        Task<(HistoryUpdateTask Task, string CompletionToken)> TryGetAsync(
            TimeSpan visibilityTimeout);
    }
}
