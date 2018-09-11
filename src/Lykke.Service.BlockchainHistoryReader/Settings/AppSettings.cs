using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.BlockchainSettings.Client;

namespace Lykke.Service.BlockchainHistoryReader.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public BlockchainHistoryReaderSettings BlockchainHistoryReaderService { get; set; }
        
        public BlockchainSettingsServiceClientSettings BlockchainSettingsServiceClient { get; set; }
    }
}
