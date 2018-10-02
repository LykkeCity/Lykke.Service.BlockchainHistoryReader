using System;

namespace Lykke.Service.BlockchainHistoryReader.Core.Domain
{
    public class HistorySource
    {
        private HistorySource(
            string address,
            string blockchainType,
            Guid clientId)
        {
            Address = address;
            BlockchainType = blockchainType;
            ClientId = clientId;
            HistoryUpdatedOn = DateTime.UnixEpoch;
            HistoryUpdateScheduledOn = DateTime.UnixEpoch;
        }


        public static HistorySource Create(
            string address,
            string blockchainType,
            Guid clientId)
        {
            return new HistorySource
            (
                address,
                blockchainType,
                clientId
            );
        }

        public static HistorySource Restore(
            string address,
            string blockchainType,
            Guid clientId,
            DateTime historyUpdatedOn,
            DateTime historyUpdateScheduledOn,
            string latestHash)
        {
            return new HistorySource(address, blockchainType, clientId)
            {
                HistoryUpdatedOn = historyUpdatedOn,
                HistoryUpdateScheduledOn = historyUpdateScheduledOn,
                LatestHash = latestHash
            };
        }
        
        
        public string Address { get; }
        
        public string BlockchainType { get; }
        
        public Guid ClientId { get; }
        
        public DateTime HistoryUpdatedOn { get; private set; }
        
        public DateTime HistoryUpdateScheduledOn { get; private set; }
        
        public string LatestHash { get; private set; }


        public void OnHistoryUpdated()
        {
            HistoryUpdatedOn = DateTime.UtcNow;
        }
        
        public void OnHistoryUpdated(
            string latestHash)
        {
            LatestHash = latestHash;

            OnHistoryUpdated();
        }

        public void OnHistoryUpdateScheduled()
        {
            HistoryUpdateScheduledOn = DateTime.UtcNow;
        }

        public void ResetLatestHash()
        {
            HistoryUpdatedOn = DateTime.UnixEpoch;
            HistoryUpdateScheduledOn = DateTime.UnixEpoch;
            LatestHash = string.Empty;
        }
    }
}
