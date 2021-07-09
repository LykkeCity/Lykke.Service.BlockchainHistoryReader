using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.BlockchainHistoryReader.AzureRepositories.Entities
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class HistorySourceEntity : AzureTableEntity
    {
        private Guid _clientId;
        private DateTime _historyUpdatedOn;
        private DateTime _historyUpdateScheduledOn;
        private string _latestHash;
        
        
        public string Address { get; set; }
        
        public string BlockchainType { get; set; }

        public Guid ClientId
        {
            get 
                => _clientId;
            set
            {
                if (_clientId != value)
                {
                    _clientId = value;
                    
                    MarkValueTypePropertyAsDirty(nameof(ClientId));
                }
            }
        }
        
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

        public string LatestHash
        {
            get 
                => _latestHash;
            set
            {
                if (_latestHash != value)
                {
                    _latestHash = value;
                    
                    MarkValueTypePropertyAsDirty(nameof(LatestHash));
                }
            }
        }
    }
}
