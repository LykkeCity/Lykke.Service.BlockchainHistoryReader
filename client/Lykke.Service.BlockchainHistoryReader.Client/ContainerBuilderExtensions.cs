using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Infrastructure;
using System;
using Lykke.Service.BlockchainHistoryReader.Client.Implementations;

#pragma warning disable 1591

namespace Lykke.Service.BlockchainHistoryReader.Client
{
    [PublicAPI]
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        ///    Registers <see cref="IBlockchainHistoryReaderClient"/> in Autofac container using <see cref="BlockchainHistoryReaderServiceClientSettings"/>.
        /// </summary>
        /// <param name="builder">
        ///    Autofac container builder.
        /// </param>
        /// <param name="settings">
        ///    BlockchainHistoryReader client settings.
        /// </param>
        /// <param name="builderConfigure">
        ///    Optional <see cref="HttpClientGeneratorBuilder"/> configure handler.
        /// </param>
        public static void RegisterBlockchainHistoryReaderClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] BlockchainHistoryReaderServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(BlockchainHistoryReaderServiceClientSettings.ServiceUrl));
            }

            var clientBuilder = HttpClientGenerator.HttpClientGenerator
                .BuildForUrl(settings.ServiceUrl)
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());

            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder.WithoutRetries();

            builder.RegisterInstance(new BlockchainHistoryReaderClient(clientBuilder.Create()))
                .As<IBlockchainHistoryReaderClient>()
                .SingleInstance();
        }
    }
}
