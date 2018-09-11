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
        
        [ProtoMember(3)]
        public string LatestHash { get; set; }
    }
}
