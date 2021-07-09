using System;
using JetBrains.Annotations;

namespace Lykke.Service.BlockchainHistoryReader.Client.Models
{
    /// <summary>
    ///    Request containing history source parameters.
    /// </summary>
    [PublicAPI]
    public class HistorySourceRequest
    {
        /// <summary>
        ///    Wallet address.
        /// </summary>
        public string Address { get; set; }
        
        /// <summary>
        ///    Blockchain type.
        /// </summary>
        public string BlockchainType { get; set; }
        
        /// <summary>
        ///    Client id.
        /// </summary>
        public Guid ClientId { get; set; }
    }
}