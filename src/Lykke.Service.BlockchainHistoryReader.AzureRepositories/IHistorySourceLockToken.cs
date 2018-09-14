using System.Threading.Tasks;

namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories
{
    public interface IHistorySourceLockToken
    {
        Task ReleaseAsync();

        Task RenewIfNecessaryAsync();
    }
}
