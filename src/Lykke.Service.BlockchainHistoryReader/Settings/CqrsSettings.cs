using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BlockchainHistoryReader.Settings
{
    public class CqrsSettings
    {
        [AmqpCheck]
        public string RabbitConnString { get; set; }
    }
}
