namespace Lykke.Service.BlockchainHistoryReader.Core.Domain
{
    public class HistoryUpdateTask
    {
        public string Address { get; set; }
        
        public string BlockchainType { get; set; }
        
        public string LatestHash { get; set; }
    }
}
