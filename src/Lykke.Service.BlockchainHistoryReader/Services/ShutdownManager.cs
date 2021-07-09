using System.Threading.Tasks;
using Lykke.Sdk;
using Lykke.Service.BlockchainHistoryReader.QueueConsumers;
using Lykke.Service.BlockchainHistoryReader.Timers;


namespace Lykke.Service.BlockchainHistoryReader.Services
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly HistoryUpdateSchedulerTimer _historyUpdateSchedulerTimer;
        private readonly HistoryUpdateTaskQueueConsumer _historyUpdateTaskQueueConsumer;

        
        public ShutdownManager(
            HistoryUpdateSchedulerTimer historyUpdateSchedulerTimer,
            HistoryUpdateTaskQueueConsumer historyUpdateTaskQueueConsumer)
        {
            _historyUpdateTaskQueueConsumer = historyUpdateTaskQueueConsumer;
            _historyUpdateSchedulerTimer = historyUpdateSchedulerTimer;
        }

        
        public async Task StopAsync()
        {
            _historyUpdateSchedulerTimer.Stop();
            
            await _historyUpdateTaskQueueConsumer.StopAsync();
        }
    }
}