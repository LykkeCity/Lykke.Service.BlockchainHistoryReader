using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Service.BlockchainHistoryReader.HistorySourceImporter.Implementations
{
    public class CommandLineOptions : ICommandLineOptions
    {
        private bool _optionsConfigured;

        private CommandOption _blockchainHistoryReaderUrl;
        private CommandOption _blockchainWalletsConnectionString;
        private CommandOption _help;


        public string BlockchainHistoryReaderUrl
            => _blockchainHistoryReaderUrl.Value();

        public string BlockchainWalletsConnectionString
            => _blockchainWalletsConnectionString.Value();
        
        public bool ShowHelp
            => _help.HasValue();
        
        
        public void Configure(
            CommandLineApplication app)
        {
            _blockchainHistoryReaderUrl = app.Option
            (
                "--blockchain-history-reader-url",
                "Lykke.Service.BlockchainHistoryReader url",
                CommandOptionType.SingleValue
            );
            
            _blockchainWalletsConnectionString = app.Option
            (
                "--blockchain-wallets-conn-string",
                "Lykke.Service.BlockchainWallets connection string",
                CommandOptionType.SingleValue
            );
            
            _help = app.HelpOption
            (
                "-h|--help"
            );
            
            _optionsConfigured = true;
        }

        public bool Validate()
        {
            if (!_optionsConfigured)
            {
                throw new Exception("Options have not been configured.");
            }

            var optionsAreValid = true;

            if (!_blockchainHistoryReaderUrl.HasValue())
            {
                Console.WriteLine("Lykke.Service.BlockchainHistoryReader url is not provided");
                
                optionsAreValid = false;
            }
            else if (!Uri.TryCreate(_blockchainHistoryReaderUrl.Value(), UriKind.Absolute, out _))
            {
                Console.WriteLine("Lykke.Service.BlockchainHistoryReader url is invalid");
                
                optionsAreValid = false;
            }
            
            if (!_blockchainWalletsConnectionString.HasValue())
            {
                Console.WriteLine("Lykke.Service.BlockchainWallets connection string is not provided");
                
                optionsAreValid = false;
            }
            
            return optionsAreValid;
        }
    }
}