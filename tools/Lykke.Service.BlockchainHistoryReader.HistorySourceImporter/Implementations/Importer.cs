using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Common.PasswordTools;
using Lykke.Common.Log;
using Lykke.Service.BlockchainHistoryReader.Client;
using Lykke.Service.BlockchainHistoryReader.Client.Models;

namespace Lykke.Service.BlockchainHistoryReader.HistorySourceImporter.Implementations
{
    public class Importer : IImporter
    {
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly ICommandLineOptions _options;

        
        public Importer(
            ILogFactory logFactory,
            ICommandLineOptions options)
        {
            _log = logFactory.CreateLog(this);
            _logFactory = logFactory;
            _options = options;
        }
        
        public async Task RunAsync()
        {
            _log.Info("Import started");
            
            var wallets = await GetWalletsAsync();
            
            _log.Info($"Importing {wallets.Length} wallets...");

            await ImportAsync(wallets);
            
            _log.Info("Import completed");
        }
        
        private async Task<WalletEntity[]> GetWalletsAsync()
        {
            var balanceRepository = new WalletRepository
            (
                _options.BlockchainWalletsConnectionString,
                _logFactory
            );
            
            var wallets = new List<WalletEntity>();

            string continuationToken = null;
            
            do
            {
                IEnumerable<WalletEntity> walletsBatch;
                
                (walletsBatch, continuationToken) = await balanceRepository.GetBalancesAsync
                (
                    100,
                    continuationToken
                );

                wallets.AddRange(walletsBatch);
                
                _log.Info($"Got {wallets.Count} non-distinct wallets to import");

            } while (continuationToken != null);

            var distinctWallets = wallets
                .GroupBy(x => new {x.IntegrationLayerId, x.Address})
                .Select(x => x.First())
                .ToArray();

            _log.Info($"Got {distinctWallets.Length} distinct wallets to import");

            return distinctWallets;
        }

        private async Task ImportAsync(
            IReadOnlyList<WalletEntity> wallets)
        {
            var client = BlockchainHistoryReaderServiceClientFactory.CreateClient
            (
                new BlockchainHistoryReaderServiceClientSettings
                {
                    ServiceUrl = _options.BlockchainHistoryReaderUrl
                },
                null
            );

            for (var i = 0; i < wallets.Count; i++)
            {
                var wallet = wallets[i];
                
                try
                {
                    await client.Api.AddHistorySourceAsync(new HistorySourceRequest
                    {
                        Address = wallet.Address,
                        BlockchainType = wallet.IntegrationLayerId
                    });
                }
                catch (Exception e)
                {
                    _log.Warning($"Failed to import wallet [blockchain type: {wallet.IntegrationLayerId}, address: {wallet.Address}].", e);
                }
                finally
                { 
                    _log.Info($"{i + 1} of {wallets.Count} wallets processed.");
                }
            }
        }
    }
}