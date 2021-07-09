using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using System;

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

            var client = BlockchainHistoryReaderServiceClientFactory.CreateClient(settings, builderConfigure);

            builder.RegisterInstance(client)
                .As<IBlockchainHistoryReaderClient>()
                .SingleInstance();
        }
    }
}
