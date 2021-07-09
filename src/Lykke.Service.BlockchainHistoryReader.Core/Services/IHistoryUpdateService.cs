using System.Threading.Tasks;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;

namespace Lykke.Service.BlockchainHistoryReader.Core.Services
{
    public interface IHistoryUpdateService
    {
        Task CompleteHistoryUpdateTaskAsync(
            HistoryUpdateTask task);

        Task<bool> ExecuteHistoryUpdateTaskAsync(
            HistoryUpdateTask task);

        Task ResetLatestHash(
            string blockchainType,
            string address);
        
        Task<HistoryUpdateTask> TryGetNextHistoryUpdateTaskAsync();
    }
}
