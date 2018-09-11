using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Client.Models;


namespace Lykke.Service.BlockchainHistoryReader.Core.Services
{
    public interface IBlockchainApiProxyService
    {
        Task<IEnumerable<HistoricalTransaction>> GetHistoryOfIncomingTransactionsAsync(
            string blockchainType,
            string address,
            string afterHash,
            int take);
    }
}
