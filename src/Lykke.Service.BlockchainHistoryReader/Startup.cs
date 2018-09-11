using JetBrains.Annotations;
using Lykke.Logs.Loggers.LykkeSlack;
using Lykke.Sdk;
using Lykke.Service.BlockchainHistoryReader.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;


namespace Lykke.Service.BlockchainHistoryReader
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "BlockchainHistoryReader API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName 
                        = "BlockchainHistoryReaderLog";
                    
                    logs.AzureTableConnectionStringResolver 
                        = settings => settings.BlockchainHistoryReaderService.Db.LogsConnString;

                    logs.Extended 
                        = extendedLogs =>
                        {
                            extendedLogs.AddAdditionalSlackChannel("CommonBlockChainIntegration", channelOptions =>
                            {
                                channelOptions.MinLogLevel = LogLevel.Information;
                            });
        
                            extendedLogs.AddAdditionalSlackChannel("CommonBlockChainIntegrationImportantMessages", channelOptions =>
                            {
                                channelOptions.MinLogLevel = LogLevel.Warning;
                                channelOptions.SpamGuard.DisableGuarding();
                            });
                        };
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
            });
        }
    }
}
