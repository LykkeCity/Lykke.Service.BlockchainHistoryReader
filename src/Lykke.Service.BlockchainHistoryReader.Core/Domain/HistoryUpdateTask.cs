using ProtoBuf;

namespace Lykke.Service.BlockchainHistoryReader.Core.Domain
{
    [ProtoContract]
    public class HistoryUpdateTask
    {
        [ProtoMember(1)]
        public string Address { get; set; }
        
        [ProtoMember(2)]
        public string BlockchainType { get; set; }
        
        [ProtoIgnore]
        public int DequeueCount { get; set; }
        
        [ProtoIgnore]
        public string Id { get; set; }
        
        [ProtoIgnore]
        public string PopReceipt { get; set; }
    }
}
