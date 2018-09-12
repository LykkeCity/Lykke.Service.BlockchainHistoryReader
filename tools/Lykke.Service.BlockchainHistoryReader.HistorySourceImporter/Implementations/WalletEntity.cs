using Lykke.AzureStorage.Tables;

namespace Lykke.Service.BlockchainHistoryReader.HistorySourceImporter.Implementations
{
    public class WalletEntity : AzureTableEntity
    {
        public string Address { get; set; }

        public string IntegrationLayerId { get; set; }
    }
}