using System.Threading.Tasks;

namespace Lykke.Service.BlockchainHistoryReader.Core.Services
{
    public interface IHistoryUpdateScheduler
    {
        Task ScheduleHistoryUpdatesAsync();
    }
}
