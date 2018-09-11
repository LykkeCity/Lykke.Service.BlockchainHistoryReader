using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories.Entities
{
    public class HistorySourceEntity : AzureTableEntity
    {
        private DateTime _historyUpdatedOn;
        private DateTime _historyUpdateScheduledOn;
        
        
        public string Address { get; set; }
        
        public string BlockchainType { get; set; }

        public DateTime HistoryUpdatedOn
        {
            get 
                => _historyUpdatedOn;
            set
            {
                if (_historyUpdatedOn != value)
                {
                    _historyUpdatedOn = value;
                    
                    MarkValueTypePropertyAsDirty(nameof(HistoryUpdatedOn));
                }
            }
        }
        
        public DateTime HistoryUpdateScheduledOn
        {
            get 
                => _historyUpdateScheduledOn;
            set
            {
                if (_historyUpdateScheduledOn != value)
                {
                    _historyUpdateScheduledOn = value;
                    
                    MarkValueTypePropertyAsDirty(nameof(HistoryUpdateScheduledOn));
                }
            }
        }
        
        public string LatestHash { get; set; }
    }
}
