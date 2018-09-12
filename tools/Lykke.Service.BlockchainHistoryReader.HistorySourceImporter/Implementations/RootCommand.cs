using System;
using System.Reflection;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.CommandLineUtils;

namespace Lykke.Service.BlockchainHistoryReader.HistorySourceImporter.Implementations
{
    public class RootCommand : IRootCommand
    {
        private readonly CommandLineApplication _app;
        private readonly IImporter _importer;
        private readonly ILog _log;
        private readonly ICommandLineOptions _options;

        
        public RootCommand(
            IImporter importer,
            ILogFactory logFactory,
            ICommandLineOptions options)
        {
            _app = new CommandLineApplication(throwOnUnexpectedArg: false);
            _importer = importer;
            _log = logFactory.CreateLog(this);
            _options = options;
        }


        public CommandLineApplication Configure()
        {
            _options.Configure(_app);

            _app.OnExecute(() => ExecuteAsync());
            
            _app.VersionOption("--version", GetVersion);
            
            return _app;
        }

        private async Task<int> ExecuteAsync()
        {
            try
            {
                if (_options.ShowHelp || !_options.Validate())
                {
                    _app.ShowHelp();
                }
                else
                {
                    await _importer.RunAsync();
                }

                return 0;
            }
            catch (Exception e)
            {
                _log.Critical(e, "Migration failed.");

                return 1;
            }
        }

        private static string GetVersion()
        {
            return typeof(RootCommand).GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;            
        }
    }
}