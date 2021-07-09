using System;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Infrastructure;
using Lykke.Service.BlockchainHistoryReader.Client.Implementations;

#pragma warning disable 1591

namespace Lykke.Service.BlockchainHistoryReader.Client
{
    [PublicAPI]
    public static class BlockchainHistoryReaderServiceClientFactory
    {
        /// <summary>
        ///    Creates <see cref="IBlockchainHistoryReaderClient"/> instance using <see cref="BlockchainHistoryReaderServiceClientSettings"/>.
        /// </summary>
        /// <param name="settings">
        ///    BlockchainHistoryReader client settings.
        /// </param>
        /// <param name="builderConfigure">
        ///    Optional <see cref="HttpClientGeneratorBuilder"/> configure handler.
        /// </param>
        public static IBlockchainHistoryReaderClient CreateClient(
            [NotNull] BlockchainHistoryReaderServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure)
        {
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

            return new BlockchainHistoryReaderClient(clientBuilder.Create());
        }
    }
}