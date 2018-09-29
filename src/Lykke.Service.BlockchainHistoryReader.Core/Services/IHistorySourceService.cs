using System;
using System.Threading.Tasks;

namespace Lykke.Service.BlockchainHistoryReader.Core.Services
{
    public interface IHistorySourceService
    {
        Task AddHistorySourceIfNotExistsAsync(
            string blockchainType,
            string address,
            Guid clientId);

        Task DeleteHistorySourceIfExistsAsync(
            string blockchainType,
            string address);
    }
}
