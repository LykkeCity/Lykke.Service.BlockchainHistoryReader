using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BlockchainHistoryReader.Client.Models;
using Refit;


namespace Lykke.Service.BlockchainHistoryReader.Client
{
    /// <summary>
    ///    BlockchainHistoryReader client API interface.
    /// </summary>
    [PublicAPI]
    public interface IBlockchainHistoryReaderApi
    {
        /// <summary>
        ///    Adds address to list of addresses with monitored history.
        /// </summary>
        [Post("/api/history-sources")]
        Task AddHistorySourceAsync(
            HistorySourceRequest request);
        
        /// <summary>
        ///    Removes address from list of addresses with monitored history.
        /// </summary>
        [Delete("/api/history-sources")]
        Task DeleteHistorySourceAsync(
            HistorySourceRequest request);

        /// <summary>
        ///    Resets last monitored transaction hash.
        /// </summary>
        [Post("/api/replay")]
        Task ReplayHistory(
            HistorySourceRequest request);
    }
}
