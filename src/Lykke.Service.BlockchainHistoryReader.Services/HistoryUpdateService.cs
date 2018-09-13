using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainHistoryReader.AzureRepositories;
using Lykke.Service.BlockchainHistoryReader.Contract;
using Lykke.Service.BlockchainHistoryReader.Contract.Events;
using Lykke.Service.BlockchainHistoryReader.Core.Domain;
using Lykke.Service.BlockchainHistoryReader.Core.Services;
using Lykke.Service.BlockchainHistoryReader.Services.Tools;
using Lykke.SettingsReader;


namespace Lykke.Service.BlockchainHistoryReader.Services
{
    [UsedImplicitly]
    public class HistoryUpdateService : IHistoryUpdateService
    {
        private readonly IBlockchainApiProxyService _blockchainApiProxy;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ReaderWriterLockSlim _enabledBlockchainTypesLock;
        private readonly IHistorySourceRepository _historySourceRepository;
        private readonly IHistoryUpdateTaskRepository _historyUpdateTaskRepository;
        private readonly ILog _log;
        
        
        public HistoryUpdateService(
            IBlockchainApiProxyService blockchainApiProxy,
            ICqrsEngine cqrsEngine,
            IHistorySourceRepository historySourceRepository,
            IHistoryUpdateTaskRepository historyUpdateTaskRepository,
            ILogFactory logFactory)
        {
            _blockchainApiProxy = blockchainApiProxy;
            _cqrsEngine = cqrsEngine;
            _enabledBlockchainTypesLock = new ReaderWriterLockSlim();
            _historySourceRepository = historySourceRepository;
            _historyUpdateTaskRepository = historyUpdateTaskRepository;
            _log = logFactory.CreateLog(this);
        }

        
        public async Task CompleteHistoryUpdateTaskAsync(
            HistoryUpdateTask task)
        {
            try
            {
                await _historyUpdateTaskRepository.CompleteAsync(task);
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to mark history update task [{task.GetIdForLog()}] as completed.");
            }
        }

        public async Task<bool> ExecuteHistoryUpdateTaskAsync(
            HistoryUpdateTask task)
        {
            try
            {
                var historySource = await _historySourceRepository.TryGetAsync
                (
                    address: task.Address,
                    blockchainType: task.BlockchainType
                );

                if (historySource != null)
                {
                    var transactions = await GetTransactionsAsync(task, historySource.LatestHash);

                    if (transactions.Any())
                    {
                        foreach (var transaction in transactions)
                        {
                            _cqrsEngine.PublishEvent
                            (
                                new TransactionCompletedEvent
                                {
                                    Amount = transaction.Amount,
                                    AssetId = transaction.AssetId,
                                    BlockchainType = task.BlockchainType,
                                    FromAddress = transaction.FromAddress,
                                    Hash = transaction.Hash,
                                    Timestamp = transaction.Timestamp,
                                    ToAddress = transaction.ToAddress,
                                    TransactionType = transaction.TransactionType
                                },
                                BoundedContext.Name
                            );
                        }

                        historySource.OnHistoryUpdated(transactions.Last().Hash);
                    }
                    else
                    {
                        historySource.OnHistoryUpdated();
                    }
                        
                        
                    await _historySourceRepository.UpdateAsync(historySource);
                }
                
                return true;
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to execute history update task [{task.GetIdForLog()}].");

                return false;
            }
        }

        public async Task ResetLatestHash(
            string blockchainType,
            string address)
        {
            var historySource = await _historySourceRepository.TryGetAsync
            (
                address: address,
                blockchainType: blockchainType
            );

            if (historySource != null)
            {
                historySource.ResetLatestHash();

                await _historySourceRepository.UpdateAsync(historySource);
            }
        }

        public async Task<HistoryUpdateTask> TryGetNextHistoryUpdateTaskAsync()
        {
            try
            {
                return await _historyUpdateTaskRepository.TryGetAsync(TimeSpan.FromMinutes(1));
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to get next history update task.");

                return null;
            }
        }
        
        private async Task<HistoricalTransaction[]> GetTransactionsAsync(
            HistoryUpdateTask task,
            string afterHash)
        {
            var transactions = new List<HistoricalTransaction>();
            
            while (true)
            {
                var transactionsSubRange = (await _blockchainApiProxy
                    .GetHistoryOfIncomingTransactionsAsync
                        (
                            blockchainType: task.BlockchainType,
                            address: task.Address,
                            afterHash: afterHash,
                            take: 100
                        ))
                    .Where(x => x.Hash != afterHash)
                    .ToList();
                
                transactions.AddRange(transactionsSubRange);

                if (!transactionsSubRange.Any())
                {
                    break;
                }
                else
                {
                    afterHash = transactionsSubRange.Last().Hash;
                }
            }
            
            
            return transactions
                .ToArray();
        }
    }
}
