using System.Threading.Tasks;
using Lykke.Cqrs;
using Lykke.Sdk;
using Lykke.Service.BlockchainHistoryReader.QueueConsumers;
using Lykke.Service.BlockchainHistoryReader.Timers;

namespace Lykke.Service.BlockchainHistoryReader.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly HistoryUpdateSchedulerTimer _historyUpdateSchedulerTimer;
        private readonly HistoryUpdateTaskQueueConsumer _historyUpdateTaskQueueConsumer;
            
        
        public StartupManager(
            ICqrsEngine cqrsEngine,
            HistoryUpdateSchedulerTimer historyUpdateSchedulerTimer,
            HistoryUpdateTaskQueueConsumer historyUpdateTaskQueueConsumer)
        {
            _cqrsEngine = cqrsEngine;
            _historyUpdateTaskQueueConsumer = historyUpdateTaskQueueConsumer;
            _historyUpdateSchedulerTimer = historyUpdateSchedulerTimer;
        }
     
        
        public Task StartAsync()
        {
            _cqrsEngine.Start();

            _historyUpdateTaskQueueConsumer.Start();

            _historyUpdateSchedulerTimer.Start();

            return Task.CompletedTask;
        }
    }
}