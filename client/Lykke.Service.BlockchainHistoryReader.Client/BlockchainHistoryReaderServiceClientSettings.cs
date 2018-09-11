using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;


namespace Lykke.Service.BlockchainHistoryReader.Client 
{
    /// <summary>
    ///    BlockchainHistoryReader client settings.
    /// </summary>
    [PublicAPI]
    public class BlockchainHistoryReaderServiceClientSettings 
    {
        /// <summary>
        ///    Service url.
        /// </summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
