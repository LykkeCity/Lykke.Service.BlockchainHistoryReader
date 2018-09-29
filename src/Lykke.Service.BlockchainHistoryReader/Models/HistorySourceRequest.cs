using System;
using JetBrains.Annotations;


namespace Lykke.Service.BlockchainHistoryReader.Models
{
    [PublicAPI]
    public class HistorySourceRequest
    {
        public string Address { get; set; }
        
        public string BlockchainType { get; set; }
        
        public Guid ClientId { get; set; }
    }
}