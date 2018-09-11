using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Service.BlockchainHistoryReader.AzureRepositories;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;
using Lykke.Service.BlockchainHistoryReader.Core.Services;
using Lykke.Service.BlockchainHistoryReader.Services.Tools;


namespace Lykke.Service.BlockchainHistoryReader.Services
{
    [UsedImplicitly]
    public class HistoryUpdateScheduler : IHistoryUpdateScheduler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly IHistorySourceLockRepository _historySourceLockRepository;
        private readonly IHistorySourceRepository _historySourceRepository;
        private readonly IHistoryUpdateTaskRepository _historyUpdateTaskRepository;
        private readonly ILog _log;

        
        public HistoryUpdateScheduler(
            IChaosKitty chaosKitty,
            IHistorySourceLockRepository historySourceLockRepository,
            IHistorySourceRepository historySourceRepository,
            IHistoryUpdateTaskRepository historyUpdateTaskRepository,
            ILogFactory logFactory)
        {
            _chaosKitty = chaosKitty;
            _historySourceLockRepository = historySourceLockRepository;
            _historySourceRepository = historySourceRepository;
            _historyUpdateTaskRepository = historyUpdateTaskRepository;
            _log = logFactory.CreateLog(this);
        }


        public async Task ScheduleHistoryUpdatesAsync()
        {
            IHistorySourceLockToken @lock = null;

            try
            {
                @lock = await _historySourceLockRepository.TryLockAsync();

                if (@lock != null)
                {
                    var now = DateTime.UtcNow;

                    _chaosKitty.Meow($"{nameof(HistoryUpdateScheduler)}-{nameof(ScheduleHistoryUpdatesAsync)}");

                    var historySources = await _historySourceRepository.GetAsync
                    (
                        historyUpdatedOnLimit: now.AddMinutes(-5),
                        historyUpdateScheduledOnLimit: now.AddHours(-1)
                    );

                    foreach (var historySource in historySources)
                    {
                        var task = new HistoryUpdateTask
                        {
                            Address = historySource.Address,
                            BlockchainType = historySource.BlockchainType,
                            LatestHash = historySource.LatestHash
                        };
                        
                        try
                        {
                            await _historyUpdateTaskRepository.EnqueueAsync(task);
                        
                            _chaosKitty.Meow(task.GetIdForLog());
                        
                            historySource.OnHistoryUpdateScheduled();

                            await _historySourceRepository.UpdateAsync(historySource);
                        }
                        catch (Exception e)
                        {
                            _log.Warning($"Failed to schedule history update task [{task.GetIdForLog()}].", e);
                        }

                        await @lock.ReleaseAsync();
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to schedule history updates.");
            }
            finally
            {
                if (@lock != null)
                {
                    await @lock.ReleaseAsync();
                }
            }
        }
    }
}
