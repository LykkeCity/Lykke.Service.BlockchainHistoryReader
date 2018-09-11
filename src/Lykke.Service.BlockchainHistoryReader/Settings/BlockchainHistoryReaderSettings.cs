using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.BlockchainHistoryReader.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BlockchainHistoryReaderSettings
    {
        public CqrsSettings Cqrs { get; set; }

        public DbSettings Db { get; set; }
        
        public IEnumerable<string> EnabledBlockchainTypes { get; set; }
        
        public int MaxDegreeOfParallelism { get; set; }
    }
}
