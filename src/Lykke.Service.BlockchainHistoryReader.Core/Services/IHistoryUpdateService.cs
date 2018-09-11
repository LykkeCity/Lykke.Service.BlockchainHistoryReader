using System.Threading.Tasks;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;

namespace Lykke.Service.BlockchainHistoryReader.Core.Services
{
    public interface IHistoryUpdateService
    {
        Task CompleteHistoryUpdateTaskAsync(
            HistoryUpdateTask task,
            string completionToken);

        Task<bool> ExecuteHistoryUpdateTaskAsync(
            HistoryUpdateTask task);

        Task<(HistoryUpdateTask Task, string CompletionToken)> TryGetNextHistoryUpdateTaskAsync();
    }
}
