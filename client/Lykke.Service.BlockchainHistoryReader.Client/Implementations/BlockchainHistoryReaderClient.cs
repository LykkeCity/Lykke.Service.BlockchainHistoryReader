using Lykke.HttpClientGenerator;

namespace Lykke.Service.BlockchainHistoryReader.Client.Implementations
{
    /// <inheritdoc />
    public class BlockchainHistoryReaderClient : IBlockchainHistoryReaderClient
    {
        /// <inheritdoc />
        public IBlockchainHistoryReaderApi Api { get; private set; }

        /// <summary>
        ///    BlockchainHistoryReaderClient constructor
        /// </summary>
        public BlockchainHistoryReaderClient(
            IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IBlockchainHistoryReaderApi>();
        }
    }
}
