using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;
using Lykke.Service.BlockchainHistoryReader.Core.Services;


namespace Lykke.Service.BlockchainHistoryReader.QueueConsumers
{
    [UsedImplicitly]
    public class HistoryUpdateTaskQueueConsumer : IStartable, IStopable
    {
        private readonly int _emptyQueueCheckInterval;
        private readonly IHistoryUpdateService _historyUpdateService;
        private readonly SemaphoreSlim _throttler;
        
        private CancellationTokenSource _cts;   
        private Task _executingTask;
        
        
        public HistoryUpdateTaskQueueConsumer(
            IHistoryUpdateService historyUpdateService,
            Settings settings)
        {
            _emptyQueueCheckInterval = settings.EmptyQueueCheckInterval;
            _historyUpdateService = historyUpdateService;
            _throttler = new SemaphoreSlim(settings.MaxDegreeOfParallelism);
        }

        public void Dispose()
        {
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                Stop();
                
                _throttler.Dispose();
                
                _cts?.Dispose();
                _executingTask?.Dispose();
            }
        }
        
        public void Start()
        {
            if (_executingTask == null)
            {
                _cts = new CancellationTokenSource();
                _executingTask = RunAsync(_cts.Token);
            }
        }

        public void Stop()
        {
            if (_executingTask != null)
            {
                _cts.Cancel(false);
            
                _executingTask.Wait();

                _executingTask.Dispose();

                _executingTask = null;
            }
        }
        
        private async Task<(bool, HistoryUpdateTask)> TryGetNextTaskAsync()
        {
            var task = await _historyUpdateService.TryGetNextHistoryUpdateTaskAsync();

            return (task != null, task);
        }

        private async Task ProcessTaskAsync(
            HistoryUpdateTask task)
        {
            if (task.DequeueCount >= 5 || await _historyUpdateService.ExecuteHistoryUpdateTaskAsync(task))
            {
                await _historyUpdateService.CompleteHistoryUpdateTaskAsync(task);
            }
        }

        private async Task ProcessTaskAndReleaseThrottlerAsync(
            HistoryUpdateTask task)
        {
            try
            {
                await ProcessTaskAsync(task);
            }
            finally
            {
                _throttler.Release();
            }
        }
        
        private async Task RunAsync(
            CancellationToken cancellationToken)
        {
            var scheduledTasks = new List<Task>();
            
            while (true)
            {
                await _throttler.WaitAsync(cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    scheduledTasks.RemoveAll(x => x.IsCompleted);

                    var (nextTaskRetrieved, nextTask) = await TryGetNextTaskAsync();

                    if (nextTaskRetrieved)
                    {
                        scheduledTasks.Add
                        (
                            ProcessTaskAndReleaseThrottlerAsync(nextTask)
                        );
                    }
                    else
                    {
                        await Task.Delay(_emptyQueueCheckInterval, cancellationToken);
                        
                        _throttler.Release();
                    }
                }
                else
                {
                    break;
                }
            }

            await Task.WhenAll(scheduledTasks);
        }

        public class Settings
        {
            public int EmptyQueueCheckInterval { get; set; }

            public int MaxDegreeOfParallelism { get; set; }
        }
    }
}
