using Lykke.Service.BlockchainHistoryReader.Core.Domain;

namespace Lykke.Service.BlockchainHistoryReader.Services.Tools
{
    internal static class HistoryUpdateTaskExtensions
    {
        public static string GetIdForLog(this HistoryUpdateTask task)
        {
            return $"{nameof(HistoryUpdateTask.BlockchainType)}: {task.BlockchainType}" +
                   $"{nameof(HistoryUpdateTask.Address)}: {task.Address}";
        }
    }
}
