using System.Threading.Tasks;

namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories
{
    public interface IHistorySourceLockRepository
    {
        Task<IHistorySourceLockToken> TryLockAsync();
    }
}
