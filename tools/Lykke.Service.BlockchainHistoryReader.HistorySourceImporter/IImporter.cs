using System.Threading.Tasks;

namespace Lykke.Service.BlockchainHistoryReader.HistorySourceImporter
{
    public interface IImporter
    {
        Task RunAsync();
    }
}