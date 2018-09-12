using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Service.BlockchainHistoryReader.HistorySourceImporter
{
    public interface IRootCommand
    {
        CommandLineApplication Configure();
    }
}