using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BlockchainHistoryReader.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BlockchainHistoryReaderSettings
    {
        [Optional]
        public ChaosSettings Chaos { get; set; }
        
        public CqrsSettings Cqrs { get; set; }

        public DbSettings Db { get; set; }
        
        public IEnumerable<string> EnabledBlockchainTypes { get; set; }
        
        public int MaxDegreeOfParallelism { get; set; }
    }
}
