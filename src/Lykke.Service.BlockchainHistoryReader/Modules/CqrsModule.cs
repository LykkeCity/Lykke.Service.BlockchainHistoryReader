using System.Collections.Generic;
using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.BlockchainHistoryReader.Contract;
using Lykke.Service.BlockchainHistoryReader.Contract.Events;
using Lykke.Service.BlockchainHistoryReader.Settings;
using Lykke.Service.BlockchainHistoryReader.Workflow;
using Lykke.Service.BlockchainWallets.Contract;
using Lykke.Service.BlockchainWallets.Contract.Events;
using Lykke.SettingsReader;
using RabbitMQ.Client;


namespace Lykke.Service.BlockchainHistoryReader.Modules
{
    [UsedImplicitly]
    public class CqrsModule : Module
    {
        private readonly AppSettings _appSettings;

        
        public CqrsModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings.CurrentValue;
        }

        
        protected override void Load(
            ContainerBuilder builder)
        {
            builder
                .Register(CreateEngine)
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
            
            builder
                .RegisterType<WalletOperationsProjection>()
                .AsSelf()
                .SingleInstance();
        }

        private CqrsEngine CreateEngine(
            IComponentContext context)
        {
            var logFactory = context.Resolve<ILogFactory>();
            
            // Messaging engine
            
            var rabbitMqSettings = new ConnectionFactory
            {
                Uri = _appSettings.BlockchainHistoryReaderService.Cqrs.RabbitConnString
            };
            
            var transportInfo = new TransportInfo
            (
                rabbitMqSettings.Endpoint.ToString(),
                rabbitMqSettings.UserName,
                rabbitMqSettings.Password,
                "None",
                "RabbitMq"
            );

            var transports = new Dictionary<string, TransportInfo>
            {
                {
                    "RabbitMq", transportInfo
                }
            };
            
            var transportResolver = new TransportResolver
            (
                transports
            );
            
            var messagingEngine = new MessagingEngine
            (
                logFactory,
                transportResolver,
                new RabbitMqTransportFactory(logFactory)
            );
            
            // Registrations
            
            var endpointResolver = new RabbitMqConventionEndpointResolver
            (
                transport: "RabbitMq",
                serializationFormat: SerializationFormat.ProtoBuf,
                environment: "lykke"
            );
            
            var registrations = new IRegistration[]
            {
                Register
                    .DefaultEndpointResolver(endpointResolver),

                Register
                    .BoundedContext(
                        BoundedContext.Name)
                    
                    .PublishingEvents(
                        typeof(TransactionCompletedEvent))
                    .With(
                        BoundedContext.EventsRoute)
                
                    .ListeningEvents(
                        typeof(WalletCreatedEvent),
                        typeof(WalletDeletedEvent))
                    .From(
                        BlockchainWalletsBoundedContext.Name)
                    .On(
                        BlockchainWalletsBoundedContext.EventsRoute)
                    .WithProjection(
                        typeof(WalletOperationsProjection),
                        BlockchainWalletsBoundedContext.Name)
            };
            
            // Cqrs Engine
            
            return new CqrsEngine
            (
                logFactory: logFactory,
                dependencyResolver: new AutofacDependencyResolver(context.Resolve<IComponentContext>()),
                messagingEngine: messagingEngine,
                endpointProvider: new DefaultEndpointProvider(),
                createMissingEndpoints: true,
                registrations: registrations
            );
        }
    }
}
