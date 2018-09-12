using System;

namespace Lykke.Service.BlockchainHistoryReader.Core.Domain
{
    public class HistorySource
    {
        private HistorySource(
            string address,
            string blockchainType)
        {
            Address = address;
            BlockchainType = blockchainType;
        }


        public static HistorySource Create(
            string address,
            string blockchainType)
        {
            return new HistorySource
            (
                address,
                blockchainType
            );
        }

        public static HistorySource Restore(
            string address,
            string blockchainType,
            DateTime historyUpdatedOn,
            DateTime historyUpdateScheduledOn,
            string latestHash)
        {
            return new HistorySource(address, blockchainType)
            {
                HistoryUpdatedOn = historyUpdatedOn,
                HistoryUpdateScheduledOn = historyUpdateScheduledOn,
                LatestHash = latestHash
            };
        }
        
        
        public string Address { get; }
        
        public string BlockchainType { get; }
        
        public DateTime HistoryUpdatedOn { get; private set; }
        
        public DateTime HistoryUpdateScheduledOn { get; private set; }
        
        public string LatestHash { get; private set; }


        public void OnHistoryUpdated(
            string latestHash)
        {
            HistoryUpdatedOn = DateTime.UtcNow;
            LatestHash = latestHash;
        }

        public void OnHistoryUpdateScheduled()
        {
            HistoryUpdateScheduledOn = DateTime.UtcNow;
        }

        public void ResetLatestHash()
        {
            LatestHash = null;
        }
    }
}
