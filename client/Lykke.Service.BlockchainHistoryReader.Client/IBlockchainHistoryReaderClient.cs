using JetBrains.Annotations;

namespace Lykke.Service.BlockchainHistoryReader.Client
{
    /// <summary>
    ///    BlockchainHistoryReader client interface.
    /// </summary>
    [PublicAPI]
    public interface IBlockchainHistoryReaderClient
    {
        /// <summary>
        ///    Application Api interface
        /// </summary>
        IBlockchainHistoryReaderApi Api { get; }
    }
}
