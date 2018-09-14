using Lykke.Service.BlockchainHistoryReader.Core.Domain;

namespace Lykke.Service.BlockchainHistoryReader.Services.Tools
{
    internal static class HistorySourceExtensions
    {
        public static string GetIdForLog(
            this HistorySource historySource)
        {
            return $"{nameof(HistorySource.BlockchainType)}: {historySource.BlockchainType}, " +
                   $"{nameof(HistorySource.Address)}: {historySource.Address}";
        }
    }
}