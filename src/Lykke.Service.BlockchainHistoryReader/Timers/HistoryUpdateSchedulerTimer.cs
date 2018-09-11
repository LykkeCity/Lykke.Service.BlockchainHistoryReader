using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.BlockchainHistoryReader.Core.Services;


namespace Lykke.Service.BlockchainHistoryReader.Timers
{
    [UsedImplicitly]
    public class HistoryUpdateSchedulerTimer : TimerPeriod
    {
        private readonly IHistoryUpdateScheduler _historyUpdateScheduler;
        
        public HistoryUpdateSchedulerTimer(
            IHistoryUpdateScheduler historyUpdateScheduler,
            ILogFactory logFactory)
        
            : base(TimeSpan.FromSeconds(60), logFactory, nameof(HistoryUpdateSchedulerTimer))
        {
            _historyUpdateScheduler = historyUpdateScheduler;
        }

        public override Task Execute(
            CancellationToken cancellationToken)
        {
            return _historyUpdateScheduler.ScheduleHistoryUpdatesAsync();
        }
    }
}
