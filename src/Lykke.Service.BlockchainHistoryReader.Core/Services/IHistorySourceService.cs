using System.Threading.Tasks;

namespace Lykke.Service.BlockchainHistoryReader.Core.Services
{
    public interface IHistorySourceService
    {
        Task AddHistorySourceIfNotExistsAsync(
            string blockchainType,
            string address);

        Task DeleteHistorySourceIfExistsAsync(
            string blockchainType,
            string address);
    }
}
