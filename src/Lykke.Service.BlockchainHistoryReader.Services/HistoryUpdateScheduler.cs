using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Service.BlockchainHistoryReader.AzureRepositories;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;
using Lykke.Service.BlockchainHistoryReader.Core.Services;
using Lykke.Service.BlockchainHistoryReader.Services.Tools;
using Lykke.SettingsReader;


namespace Lykke.Service.BlockchainHistoryReader.Services
{
    [UsedImplicitly]
    public class HistoryUpdateScheduler : IHistoryUpdateScheduler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly IReloadingManager<IEnumerable<string>> _enabledBlockchainTypesManager;
        private readonly IHistorySourceLockRepository _historySourceLockRepository;
        private readonly IHistorySourceRepository _historySourceRepository;
        private readonly IHistoryUpdateTaskRepository _historyUpdateTaskRepository;
        private readonly ILog _log;


        private HashSet<string> _enabledBlockchainTypes;
        private DateTime _enabledBlockchainTypesExpiresOn;
        
        public HistoryUpdateScheduler(
            IChaosKitty chaosKitty,
            IHistorySourceLockRepository historySourceLockRepository,
            IHistorySourceRepository historySourceRepository,
            IHistoryUpdateTaskRepository historyUpdateTaskRepository,
            ILogFactory logFactory,
            Settings settings)
        {
            _chaosKitty = chaosKitty;
            _enabledBlockchainTypesManager = settings.EnabledBlockchainTypesManager;
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

                    if (_enabledBlockchainTypesExpiresOn <= now)
                    {
                        await _enabledBlockchainTypesManager.Reload();
                        
                        _enabledBlockchainTypes = new HashSet<string>(_enabledBlockchainTypesManager.CurrentValue);

                        _enabledBlockchainTypesExpiresOn = now.AddMinutes(5);
                    }
                    
                    foreach (var historySource in historySources)
                    {
                        if (!_enabledBlockchainTypes.Contains(historySource.BlockchainType))
                        {
                            continue;
                        }
                        
                        var task = new HistoryUpdateTask
                        {
                            Address = historySource.Address,
                            BlockchainType = historySource.BlockchainType
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
        
        public class Settings
        {
            public IReloadingManager<IEnumerable<string>> EnabledBlockchainTypesManager { get; set; }
        }
    }
}
