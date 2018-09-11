using System.Linq;
using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Service.BlockchainHistoryReader.AzureRepositories;
using Lykke.Service.BlockchainHistoryReader.AzureRepositories.Implementations;
using Lykke.Service.BlockchainHistoryReader.Core.Services;
using Lykke.Service.BlockchainHistoryReader.QueueConsumers;
using Lykke.Service.BlockchainHistoryReader.Services;
using Lykke.Service.BlockchainHistoryReader.Settings;
using Lykke.Service.BlockchainHistoryReader.Timers;
using Lykke.Service.BlockchainSettings.Client.HttpClientGenerator;
using Lykke.SettingsReader;


namespace Lykke.Service.BlockchainHistoryReader.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(
            ContainerBuilder builder)
        {
            LoadRepositories(builder);
            
            LoadServices(builder);

            LoadExternalDependencies(builder);
            
            LoadWorkers(builder);
        }

        private void LoadExternalDependencies(
            ContainerBuilder builder)
        {
            var currentSettings = _appSettings.CurrentValue;
            
            // BlockchainSettingsClient

            builder
                .Register(x =>
                {
                    var factory = new BlockchainSettingsClientFactory();

                    return factory.CreateNew
                    (
                        settings: currentSettings.BlockchainSettingsServiceClient
                    );
                });
            
            // ChaosKitty
            
            builder
                .RegisterChaosKitty(currentSettings.BlockchainHistoryReaderService.Chaos);
        }

        private void LoadRepositories(
            ContainerBuilder builder)
        {
            var connectionString = _appSettings.ConnectionString(x => x.BlockchainHistoryReaderService.Db.DataConnString);
            
            // HistorySourceLockRepository
            
            builder
                .Register(x => HistorySourceLockRepository.Create
                (
                    connectionString: connectionString
                ))
                .As<IHistorySourceLockRepository>()
                .SingleInstance();
            
            // HistorySourceRepository
            
            builder
                .Register(x => HistorySourceRepository.Create
                (
                    connectionString: connectionString,
                    logFactory: x.Resolve<ILogFactory>(),
                    chaosKitty: x.Resolve<IChaosKitty>()
                ))
                .As<IHistorySourceRepository>()
                .SingleInstance();
            
            // HistoryUpdateTaskRepository
            
            builder
                .Register(x => HistoryUpdateTaskRepository.Create
                (
                    connectionString: connectionString
                ))
                .As<IHistoryUpdateTaskRepository>()
                .SingleInstance();
        }

        private void LoadServices(
            ContainerBuilder builder)
        {
            // BlockchainApiProxyService

            builder
                .RegisterType<BlockchainApiProxyService>()
                .As<IBlockchainApiProxyService>()
                .SingleInstance();
            
            // HistorySourceService
            
            builder
                .RegisterType<HistorySourceService>()
                .As<IHistorySourceService>()
                .SingleInstance();
            
            // HistoryUpdateScheduler
            
            builder
                .RegisterType<HistoryUpdateScheduler>()
                .As<IHistoryUpdateScheduler>()
                .SingleInstance();
            
            // HistoryUpdateService
            
            var enabledBlockchainTypes = _appSettings.CurrentValue
                .BlockchainHistoryReaderService
                .EnabledBlockchainTypes
                .ToArray();
            
            builder
                .RegisterInstance(new HistoryUpdateService.Settings
                {
                    EnabledBlockchainTypes = enabledBlockchainTypes
                })
                .AsSelf();
            
            builder
                .RegisterType<HistoryUpdateService>()
                .As<IHistoryUpdateService>()
                .SingleInstance();
        }

        private void LoadWorkers(
            ContainerBuilder builder)
        {
            // HistoryUpdateSchedulerTimer
            
            builder
                .RegisterType<HistoryUpdateSchedulerTimer>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();
            
            // HistoryUpdateTaskQueueConsumer

            builder
                .RegisterInstance(new HistoryUpdateTaskQueueConsumer.Settings
                {
                    EmptyQueueCheckInterval = 30 * 1000,
                    MaxDegreeOfParallelism = _appSettings.CurrentValue.BlockchainHistoryReaderService.MaxDegreeOfParallelism
                });
            
            builder
                .RegisterType<HistoryUpdateTaskQueueConsumer>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();
        }
    }
}
