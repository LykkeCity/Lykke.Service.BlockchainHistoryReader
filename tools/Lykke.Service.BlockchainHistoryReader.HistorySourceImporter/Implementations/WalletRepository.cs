using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.SettingsReader.ReloadingManager;


namespace Lykke.Service.BlockchainHistoryReader.HistorySourceImporter.Implementations
{
    public class WalletRepository
    {
        private readonly INoSQLTableStorage<WalletEntity> _wallets;
        
        public WalletRepository(
            string connectionString,
            ILogFactory logFactory)
        {
            _wallets = AzureTableStorage<WalletEntity>.Create
            (
                connectionStringManager: ConstantReloadingManager.From(connectionString),
                tableName: "Wallets",
                logFactory: logFactory
            );
        }


        public Task<(IEnumerable<WalletEntity> balances, string continuationToken)> GetBalancesAsync(
            int take,
            string continuationToken)
        {
            return _wallets.GetDataWithContinuationTokenAsync(take, continuationToken);
        }
    }
}