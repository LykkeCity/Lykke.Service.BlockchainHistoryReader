using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Service.BlockchainHistoryReader.HistorySourceImporter
{
    public interface ICommandLineOptions
    {
        string BlockchainHistoryReaderUrl { get; }
        
        string BlockchainWalletsConnectionString { get; }
        
        bool ShowHelp { get; }
        
        
        void Configure(
            CommandLineApplication app);

        bool Validate();
    }
}